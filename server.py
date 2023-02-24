import socket
from threading import Thread
import json
import random

from flask import Flask
from flask_restful import Resource, Api, reqparse

import firebase_admin
from firebase_admin import credentials
from firebase_admin import firestore

# Firebase credentials setup
cred = credentials.Certificate(
    "firebase-certificate.json"
)
firebase_admin.initialize_app(cred)

db = firestore.client()


class Server(Thread):
    def __init__(self, ip, port, rooms):
        Thread.__init__(self)
        self.tcpServer = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.tcpServer.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.tcpServer.bind((ip, port))

        self.clients = set()
        self.rooms = rooms

    def run(self):
        while True:
            self.tcpServer.listen(4)
            print("Waiting for connections from TCP clients...")
            (conn, (ip, port)) = self.tcpServer.accept()
            self.clients.add(conn)
            newthread = ClientThread(self, conn, ip, port)
            newthread.start()


class ClientThread(Thread):

    BUFFER_SIZE = 1024
    MSG_DELIMITER = "\n"

    def __init__(self, server, connection, ip, port):
        Thread.__init__(self)
        self.ip = ip
        self.port = port
        self.conn = connection
        self.server = server

        self.room_code = None
        self.user_id = None
        self.username = None
        print("[+] New server socket thread started for " + ip + ":" + str(port))

    def run(self):
        msg_buffer = ""
        while True:
            data = self.safe_recv(ClientThread.BUFFER_SIZE)

            if not data:
                self.handle_client_disconnect()
                return

            data = data.decode("utf8")

            # print(f"Received message from {self.ip}:{self.port}: {data}")
            start_message = 0
            msg_buffer += data
            for i in range(len(msg_buffer)):
                if msg_buffer[i] == ClientThread.MSG_DELIMITER:
                    # print("received message " + msg_buffer)
                    self.handle_message(msg_buffer[start_message : i + 1])
                    start_message = i + 1
            if msg_buffer[-1] != ClientThread.MSG_DELIMITER:
                # The message has not ended
                msg_buffer = msg_buffer[start_message:]
            else:
                msg_buffer = ""

    def handle_message(self, msg: str):
        msg_dict = json.loads(msg)
        exclude_self = True
        if msg_dict["type"] == "JoinRoomEvent":
            success = self.handle_client_join(msg_dict)
            if not success:
                return
        elif msg_dict["type"] == "StartGameEvent":
            exclude_self = False
            self.server.rooms[self.room_code]["started"] = True

        # Forward the message to clients in the same room
        self.send_to_room(self.room_code, msg, exclude_self=exclude_self)

    def handle_client_join(self, msg: dict):
        room_code = msg["code"]
        user_id = msg["userId"]
        username = msg["username"]

        if room_code not in self.server.rooms:
            print(room_code + " does not exist")
            self.send_error("The room " + room_code + " does not exist")
            return False
        else:
            players = self.server.rooms[room_code]["players"]
            if len(players) == self.server.rooms[room_code]["maxPlayers"]:
                print(room_code + " is full")
                self.send_error("The room " + room_code + " is full")
                return False
            else:
                players[user_id] = {
                    "conn": self.conn,
                    "userId": user_id,
                    "username": username,
                }

                self.room_code = room_code
                self.user_id = user_id
                self.username = username

                # Send players already in the room
                for user_id in players:
                    player = players[user_id]
                    if player["conn"] != self.conn:
                        join_event = json.dumps(
                            {
                                "type": "JoinRoomEvent",
                                "code": room_code,
                                "userId": player["userId"],
                                "username": player["username"],
                            }
                        )
                        join_event += "\n"
                        self.safe_send(join_event.encode("utf8"))
        return True

    def send_error(self, reason: str):
        error_json = json.dumps(
            {
                "status": "error",
                "reason": reason,
            }
        )
        self.safe_send(error_json.encode("utf8"))

    def send_to_room(self, room_code, msg: str, exclude_self=True):
        players = self.server.rooms[room_code]["players"]
        for user_id in players:
            player = players[user_id]
            conn = player["conn"]
            if exclude_self and conn == self.conn:
                continue
            try:
                # print("sending ", msg.encode("utf8"))
                conn.send(msg.encode("utf8"))
            except Exception as e:
                print("Si è verificato un Matteo")
                print(e)

    def handle_client_disconnect(self):
        print("Connection closed with client", self.ip, self.port)
        if self.room_code:
            event = json.dumps(
                {
                    "type": "LeaveRoomEvent",
                    "code": self.room_code,
                    "userId": self.user_id,
                    "username": self.username,
                }
            )
            event += "\n"

            self.send_to_room(self.room_code, event)

            room_code = self.room_code
            # Reset room_code in order to don't remove the user twice
            self.room_code = None

            del self.server.rooms[room_code]["players"][self.user_id]

            if len(self.server.rooms[room_code]["players"]) == 0:
                del self.server.rooms[room_code]

    def safe_send(self, data):
        try:
            self.conn.send(data)
        except Exception as _:
            self.handle_client_disconnect()

    def safe_recv(self, buffer_size):
        try:
            data = self.conn.recv(buffer_size)
            return data
        except Exception as _:
            return None


def launch_rest_server(host, port, rooms):
    """
    Server RESTful
    Available endpoints:
    /rooms
        GET - Retrieve all the rooms that are not full
    /room
        PUT (name) - Create a new room and retrieve its code (roomCode)
        DELETE (roomCode) - Delete the room with (roomCode) as code
        POST (roomCode) - Join in the room with (roomCode) as code
    """
    room_id = 0

    def generate_room_code(length=5):
        uppercase_letters = [
            chr(n_letter) for n_letter in range(ord("A"), ord("Z") + 1)
        ]

        generated = False
        while not generated:
            room_code = ""
            for _ in range(length):
                room_code += random.choice(uppercase_letters)
            generated = room_code not in rooms
        return room_code

    class ScoresList(Resource):
        def get(self):
            return {
                "money": self.get_scores("money"),
                "enemy": self.get_scores("money"),
            }

        def get_scores(self, score_type: str):
            score_ref = db.collection(score_type)
            query = score_ref.order_by(
                "score", direction=firestore.Query.DESCENDING
            ).limit(5)
            results = query.stream()
            return [res.to_dict() for res in results]

    class Scores(Resource):
        def get(self, score_type: str):
            score_ref = db.collection(score_type)
            query = score_ref.order_by(
                "score", direction=firestore.Query.DESCENDING
            ).limit(5)
            results = query.stream()
            scores = [res.to_dict() for res in results]
            return {"scores": scores}

        def put(self, score_type: str):
            parser = reqparse.RequestParser()
            parser.add_argument("username")
            parser.add_argument("userId")
            parser.add_argument("score", type=int)

            args = parser.parse_args()
            self.increase_score(
                score_type, args["score"], args["username"], args["userId"]
            )

        def increase_score(
            self, score_type: str, score: int, username: str, userId: str
        ):
            doc_ref = db.collection(score_type).document(userId)
            doc_ref.set(
                {
                    "score": firestore.Increment(score),
                    "username": username,
                    "userId": userId,
                },
                merge=True,
            )

    class Rooms(Resource):
        def get(self):
            available_rooms = [
                {
                    "code": room_code,
                    "name": rooms[room_code]["name"],
                    "map": rooms[room_code]["map"],
                    "maxPlayers": rooms[room_code]["maxPlayers"],
                }
                for room_code in rooms
                if not rooms[room_code]["started"]
                and len(rooms[room_code]["players"]) < rooms[room_code]["maxPlayers"]
            ]
            return {"rooms": available_rooms}

    class Room(Resource):
        def post(self):
            parser = reqparse.RequestParser()
            parser.add_argument("name")
            parser.add_argument("map", type=int)
            parser.add_argument("maxPlayers", type=int)

            args = parser.parse_args()
            room_name = args["name"]
            room_map = args["map"]
            room_max_players = args["maxPlayers"]
            room_code = generate_room_code()
            print("Create room:", args)
            rooms[room_code] = {
                "name": room_name,
                "players": {},
                "map": room_map,
                "maxPlayers": room_max_players,
                "started": False,
            }
            return {"status": "success", "code": room_code}

        def delete(self, roomCode):
            if room_id in rooms:
                del rooms[roomCode]
                return {"status": "success", "code": roomCode}
            else:
                return {
                    "status": "error",
                    "reason": "Room " + roomCode + " does not exist",
                }

    app = Flask(__name__)
    api = Api(app)

    api.add_resource(Room, "/room")
    api.add_resource(Rooms, "/rooms")
    api.add_resource(ScoresList, "/scores")
    api.add_resource(Scores, "/scores/<string:score_type>")

    app.run(debug=False, host=host, port=port)


if __name__ == "__main__":
    rooms = {}

    host = "0.0.0.0"
    socket_port = 2042
    api_port = 2043

    Server(host, socket_port, rooms).start()
    launch_rest_server(host, api_port, rooms)
