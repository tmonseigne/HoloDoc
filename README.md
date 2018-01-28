# HoloDoc - Graduation Project

HoloDoc is a document classification application using augmented reality. This application is meant to be used with Microsoft **HoloLens**.

# Setup application

## System requirements
To be able to develop on our application, there is a requirement configuration, defined in the part [System requirements](https://developer.microsoft.com/en-us/windows/mixed-reality/install_the_tools#system_requirements).

In summary:
 * You need a 64-bit Windows 10 Pro, Enterprise or Education.
 * 8GB of RAM or more
 * In the BIOS, the following features must be supported and enabled:
    * Hardware-assisted virtualization
    * Second Level Address Translation (SLAT)
    * Hardware-based Data Execution Prevention (DEP)
  * GPU (The emulator might work with an unsupported GPU, but will be significantly slower)
    * DirectX 11.0 or later
    * WDDM 1.2 driver or later

Moreover, you have to ensure that the "Hyper-V" feature has been enabled on your system. : Go Control Panel > Programs > Programs and Features > Turn Windows Features on or off > ensure that "Hyper-V" is selected for the Emulator installation to be successful.

## Installation for HoloLens and immersive headsets

Now, you have to install different softwares described in the part [Installation checklist for HoloLens](https://developer.microsoft.com/en-us/windows/mixed-reality/install_the_tools#installation_checklist_for_hololens) and the part [Installation checklist for immersive headsets](https://developer.microsoft.com/en-us/windows/mixed-reality/install_the_tools#installation_checklist_for_immersive_headsets).

In summary:
 * Enable Developer mode : Go to Settings > Update & security > For developers
 * Download and install [Windows 10 Fall Creators Update](https://www.microsoft.com/en-us/software-download/windows10)
 * Download and install [Visual Studio 2017](https://developer.microsoft.com/en-us/windows/downloads). During the installation, select the following workloads : "Universal Windows Platform development", "Game Development with Unity" and "Desktop development with C++"
 * Download and install [Windows 10 Fall Creators Update SDK](https://developer.microsoft.com/en-US/windows/downloads/windows-10-sdk)
 * Download and install [HoloLens Emulator and Holographic Templates](https://go.microsoft.com/fwlink/?linkid=852626)
 * Download and install [Unity 2017](https://store.unity.com/download) and select "Windows Store .NET Scripting Backend" during the installation
 * [Update graphics drivers](https://developer.microsoft.com/en-us/windows/mixed-reality/updating_your_gpu_driver)

## Installation of Node.js and launching the server

Download and install [Node.js](https://nodejs.org/en/download/). After that, launch the program "Node.js command prompt" and go to the folder `HoloDocServer` with the command `cd [APP_PATH]HoloDocServer`. Then, type the command `npm start` to install and launch the server.
