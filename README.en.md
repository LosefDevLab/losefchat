# LosefChat | A Simple .NET Local Chat Module

**Primary Developer**: ALong

**Copyright Holders**:
Copyright (C) 2025 LosefDevLab
Copyright (C) 2018-Present XYLCS/XIT
Copyright (C) 2019-2025 kakako Chat Community Studio (KCCS)
Copyright (C) 2024-2025 PPPO Technological Team (PTT)

Collectively referred to as the **LosefChat Development Team**

*LosefChat (2025) by LosefChat Development Team Freedom-create in XFCSTD.*<br>
*XFCSTD PATH: /XFCSTD.license*

> **IMPORTANT**: This project requires manual compilation. Refer to the compilation guide at the end.
> **Hint**: From 113th commit onwards, all code is licensed under GPL.

---

## Core Features

- **Modular Architecture**
- **MOD Support**
- **IPv4/IPv6 Dual Protocol**
- **TLS Encryption**
- **Command-Line Interface**
- **Low Resource Consumption**
- **Password Protection**
- **Anti-Passcracking PEFENDER System**
- **Server Administration Tools**
- **Open-Source Freedom**

---

## Easy Deployment

**Server Launch**:

1. Enter `2` followed by port number to start server.

**Intranet Penetration**:

- Public IP users require no additional configuration.

**Command Control**:

- Execute server operations through simple text commands.

**Client Connection**:

- Type `1`, then enter IP address and port to connect.

**Graceful Disconnect**:

- Input `exit` to send automated goodbye message.
  *(but most users still prefer closing console directly)*

---

## Module System

LosefChat supports powerful plugin architecture through:

1. Cloning LosefChat repository
2. Copying modules to designated development area
3. Adding installation code in runtime directory
4. Recompiling LosefChat for module activation

---

## XSC Development Termination

We officially discontinue XShChat (XSC) development to create a new generation chat solution due to:

- Severe version control chaos
- Incomplete feature set
- Months-long stagnation
- The open source repo is not updated for a long time
- Poor user experience leading to third-party alternatives

While servers like `kkko` and `pppo` still operate with XSC, we commit to improving through:

- Unified server-client architecture
- Enhanced filesystem optimization
- Friendlier UUE
- Critical new features:
  - IPv4/IPv6 support
  - Advanced logging with search capability
  - Modular development framework
  - Password protection (Fulfill a legacy request from one of once a good friend...)

---

## Feature Comparison: LosefChat vs XShChat


| Feature                | LosefChat                               | XShChat                                   |
| ---------------------- | --------------------------------------- | ----------------------------------------- |
| **UI Interface**       | Command-line                            | Command-line                              |
| **User Management**    | Ban/kick/unban with behavior tracking   | Similar functionality                     |
| **Logging**            | Detailed logs with search functionality | Standard logging                          |
| **Protocol Support**   | IPv4/IPv6 dual-stack                    | IPv4-only                                 |
| **Cross-Platform**     | Windows/Linux/macOS support             | Limited macOS compatibility               |
| **Modularity**         | Active MOD development ecosystem        | Deprecated MOD system                     |
| **Security**           | TLS 1.2 encryption                      | No encryption (critical vulnerability)    |
| **Privacy Protection** | Removed data collection code            | Contains privacy-invading data harvesters |
| **Community Support**  | Growing developer community             | Fragmented contributor base               |

---

## Compilation Guide

**Prerequisites**:

- .NET 8.0+
- OpenSSL 3.0+
- Git

**Steps**:

```bash
git clone https://github.com/LosefDevLab/losefchat.git
cd losefchat
dotnet build
cd bin/Debug/net8.0
# Server-only security setup
openssl genpkey -algorithm RSA -out sfc.key -aes256
openssl req -new -key sfc.key -out sfc.csr
openssl x509 -req -days 365 -in sfc.csr -signkey sfc.key -out sfc.crt
openssl pkcs12 -export -out sfc.pfx -inkey sfc.key -in sfc.crt
# In practice, it is recommended to use a trusted signature source like Microsoft
./losefchat
```

---

## Contribution Standards

**Commit Message Format**:

- Create issues for feature/fix first
- Use these prefixes:
  - `Add for #x: `
  - `Update for #x: `
  - `Fix in #x: `
  - `Merge branch [name] into [target] for #x: `

**Release Versioning**:

- Follow WVPB versioning scheme
- You don't need to annotate the markupUntagged releases
- MD-formatted release notes:
  ```markdown
  ## [Version] Release Notes

  ### Key Updates:
  - (Summarize all CMTMSG entries)
  ```
