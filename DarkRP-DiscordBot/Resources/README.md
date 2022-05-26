# Resources
## Icons folder
Images in .png format that are used for the embed thumbnails (following the model path).

## Files
Following are samples of what files the code expects and its contents.

## discord.txt
```
Replace this line with the testing bot token (used in Debug)
Replace this line with the testing guild ID (used in Debug)
Replace this line with the bot token (used in Release)
Replace this line with the roles that can use /darkrp status-set separated by commas (used in Debug)
Replace this line with the roles that can use /darkrp status-set separated by commas (used in Release)
```

## jobs.lua
```
TEAM_CITIZEN = DarkRP.createJob("Citizen", {
    color = Color(20, 150, 20, 255),
    model = {
        "models/player/Group01/Female_01.mdl",
        "models/player/group01/male_01.mdl"
    },
    description = [[The Citizen is the most basic level of society you can hold besides being a hobo. You have no specific role in city life.]],
    weapons = {},
    command = "citizen",
    max = 0,
    salary = GAMEMODE.Config.normalsalary,
    admin = 0,
    vote = false,
    hasLicense = false,
    candemote = false,
    category = "Citizens",
})

TEAM_POLICE = DarkRP.createJob("Civil Protection", {
    color = Color(25, 25, 170, 255),
    model = {"models/player/police.mdl", "models/player/police_fem.mdl"},
    description = [[The protector of every citizen that lives in the city.
        You have the power to arrest criminals and protect innocents.
        Hit a player with your arrest baton to put them in jail.
        Bash a player with a stunstick and they may learn to obey the law.
        The Battering Ram can break down the door of a criminal, with a warrant for their arrest.
        The Battering Ram can also unfreeze frozen props (if enabled).
        Type /wanted <name> to alert the public to the presence of a criminal.]],
    weapons = {"arrest_stick", "unarrest_stick", "weapon_glock2", "stunstick", "door_ram", "weaponchecker"},
    command = "cp",
    max = 4,
    salary = GAMEMODE.Config.normalsalary * 1.45,
    admin = 0,
    vote = true,
    hasLicense = true,
    ammo = {
        ["pistol"] = 60,
    },
    category = "Civil Protection",
})
```

## shipments.lua
```
DarkRP.createShipment("P228", {
    model = "models/weapons/w_pist_p228.mdl",
    entity = "weapon_p2282",
    price = 0,
    amount = 10,
    separate = true,
    pricesep = 185,
    noship = true,
    allowed = {TEAM_GUN},
    category = "Pistols",
})

DarkRP.createShipment("AK47", {
    model = "models/weapons/w_rif_ak47.mdl",
    entity = "weapon_ak472",
    price = 2450,
    amount = 10,
    separate = false,
    pricesep = nil,
    noship = false,
    allowed = {TEAM_GUN},
    category = "Rifles",
})
```

## weapons_dict.txt
```
internal_weapon_name = String Name
weapon_physcannon = Gravity Gun
```
