Here is project Orbitality - the test task written in 4 hours that represents how we usually develop games.
In developing we use a simple "gameplay logic model" + "view" architecture.
And instead of pure OOP aproach, we used a more balanced aproach where Procedural/Functional/OOP are used according to problem we solve.
So some popular programing belives are intentionaly not used. 

The gameplay model is often implemented using reactive data library ZergRush which is 8 years in development and thriving and prooved on lots of different projects.
ZergRush is written and supported by our team. 
Here is the repository https://github.com/CeleriedAway/ZergRush
You can look through unity samples in package manager for ZergRush packet.
Also, ZergRush provides a powerful code-generation solution to solve various data tasks like binary/json serialization, hashing, deep copy ect...
Orbitality project uses old version of ZergRush and to review library itself it is better to checkout fresh version from git. 

For multiplayer we developed and used our own Deterministic Predict/Rollback Multiplayer Engine (a concept similar to Photon Quantum but implementation is much more simple and userfriendly) which is perfect for session-based games, MOBAs, RPGs, Strategies, turn-based games, and some MMO games
We use gameplay logic + data as a shared module on the .net server and in the unity client.
All those technologies are high security and anti-cheat by design which make them applicable for competitive games and crypto projects.
Contact if you need a demo of those technologies.
