# Suborner: The Invisible Account Forger

![image info](https://r4wsec.com/notes/the_suborner_attack/images/suborner_banner.png)

[![Arsenal](https://github.com/toolswatch/badges/blob/master/arsenal/usa/2022.svg)](https://www.blackhat.com/us-22/arsenal/schedule/#suborner-a-windows-bribery-for-invisible-persistence-27976)

## What's this?

A simple program to create a Windows account you will only know about :)

- Create invisible local accounts without `net user` or Windows OS user management applications (e.g. `netapi32::netuseradd`)
- Works on all Windows NT Machines (Windows XP to 11, Windows Server 2003 to 2022)
- Impersonate through [RID Hijacking](https://r4wsec.com/notes/rid_hijacking/index.html) any existing account (enabled or disabled) after a successful authentication 

Create an invisible machine account with administrative privileges, and without invoking that annoying Windows Event Logger to report its creation!

## Where can I see more?

Released at [Black Hat USA 2022: Suborner: A Windows Bribery for Invisible Persistence](https://www.blackhat.com/us-22/arsenal/schedule/index.html#suborner-a-windows-bribery-for-invisible-persistence-27976)

- Blogpost: [R4WSEC - Suborner: A Windows Bribery for Invisible Persistence](https://r4wsec.com/notes/the_suborner_attack/index.html)
- Demo: [YouTube - Suborner: Creation of Invisible Account on Windows 11](https://youtu.be/TKIHRhaO5tk)
- Slides - [HITB Singapore Main Track - Suborner Slides](https://conference.hitb.org/hitbsecconf2022sin/materials/D2T1%20-%20Suborner%20-%20Windows%20Bribery%20for%20Invisible%20Persistence%20-%20Sebastian%20Castro.pdf)
- Paper: [ACM CCS Checkmate 24. Ghost in the SAM: Stealthy, Robust, and Privileged Persistence through Invisible Accounts](https://dl.acm.org/doi/10.1145/3689934.3690839)
```
@inproceedings{10.1145/3689934.3690839,
author = {Castro, Sebasti\'{a}n R. and C\'{a}rdenas, Alvaro A.},
title = {Ghost in the SAM: Stealthy, Robust, and Privileged Persistence through Invisible Accounts},
year = {2024},
isbn = {9798400712302},
publisher = {Association for Computing Machinery},
address = {New York, NY, USA},
url = {https://doi.org/10.1145/3689934.3690839},
doi = {10.1145/3689934.3690839},
pages = {59–72},
numpages = {14},
}
```

## How can I use this?
### Build
- Make sure you have .NET 4.0 and Visual Studio 2019
- Clone this repo: `git clone https://github.com/r4wd3r/Suborner/`
- Open the .sln with Visual Studio
- Build x86, x64 or both versions
- Bribe Windows!

### Release 
Download the [latest release](https://github.com/r4wd3r/Suborner/releases) and pwn!

### Usage
```
 _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _

      88
  .d88888b.                  S U B O R N E R
 d88P 88"88b
 Y88b.88        The Invisible Account Forger
 "Y88888b.                        by @r4wd3r
      88"88b                          v1.0.1
 Y88b 88.88P
  "Y88888P"               https://r4wsec.com
      88
 _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _

Description:

    A stealthy tool to create invisible accounts on Windows systems.

Parameters:

    USERNAME: Username for the new suborner account. Default = <HOSTNAME>$
    Syntax: /username:[string]

    PASSWORD: Password for the new suborner account. Default = Password.1
    Syntax: /password:[string]

    RID: RID for the new suborner account. Default = Next RID available
    Syntax: /rid:[decimal int]

    RIDHIJACK: RID of the account to impersonate. Default = 500 (Administrator)
    Syntax: /ridhijack:[decimal int]

    TEMPLATE: RID of the account to use as template for the new account creation. Default = 500 (Administrator)
    Syntax: /template:[decimal int]

    MACHINEACCOUNT: Forge as machine account for extra stealthiness. Default = yes
    Syntax: /machineaccount:[yes/no]

    DEBUG: Enable debug mode for verbose logging. Default = disabled
    Syntax: /debug
```

## Thanks!
This attack would not have been possible without the great research done by:
- Benjamin Delpy ([@gentilkiwi](https://twitter.com/gentilkiwi)) and his outstanding [Mimikatz](https://github.com/gentilkiwi/mimikatz)
- The [SecureAuth](https://www.secureauth.com/) researchers behind [Impacket](https://github.com/SecureAuthCorp/impacket)
- Ben Ten [@Ben0xA](https://twitter.com/Ben0xA)
- Infosec community!

# What's next?
~Hack~ Suborn the planet!
