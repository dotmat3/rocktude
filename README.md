# 🌑 Rocktude
**Rocktude** is a tower defense game for **Android** developed using the **Unity** engine.

The objective of the game is to destroy all the rocks and avoid that they reach the end of the path. The game lasts for **40 round** with a special **final boss**.

**Three maps** are available, each one in a completly different setting.
The player can purchase and place several **turrets** each one with its own characteristics in order to fight the wave of rocks that become each round stronger.
Each turret can be **upgraded** three times and a special powerful **missle** can be controlled using the gyroscope of the phone.

Beware sometimes a malus can be activated and some of your turrets may run out of ammo! You will need to use the gyroscope of your phone guiding an ammo crate to refill the disabled turrets.

Moreover, it is also possibile to play Rocktude with other people in the **multiplayer** mode where 2, 3 or 4 people can fight to reach the last round and beat the final boss.
You can **create a new room** or join one using the **server browser** or by scanning the **QR code** from the screen of a friend.

Finally, players can also try to reach the top of the **leaderboard** achieving the highest number of collected money or enemies killed.

<div align="center">

https://user-images.githubusercontent.com/58000595/221223806-b7485eff-0315-4fa7-8128-6bb61a60469a.mp4

</div>
<br />

The content of the repo is a common Unity project with the addition of the `server.py` file which contains the code in order to run the Python server needed by Rocktude.

## Contributors

<a href="https://github.com/SkyLionx" target="_blank">
  <img src="https://img.shields.io/badge/Profile-Fabrizio%20Rossi-green?style=for-the-badge&logo=github&labelColor=blue&color=white">
</a>
<br /><br />
<a href="https://github.com/dotmat3" target="_blank">
  <img src="https://img.shields.io/badge/Profile-Matteo%20Orsini-green?style=for-the-badge&logo=github&labelColor=blue&color=white">
</a>


## Technologies

In this project the following technologies were used:
- **Unity C#**
- **ZXing** for QR decoding
- **Flask** for the Python server
- **TCP Sockets** in order to send multiplayer events

Finally, **Firebase** was used for the authentication system using **Google SignIn**.
