# Tuningfork Unity plugin
Copyright 2020 Google LLC. All rights reserved.

Tuningfork is a library to help developers measure frame rendering time across different devices and game stages. This plugin will help developers integrate Tuningfork into their game.

# Release Notes
The release notes are available on the [releases page](https://github.com/android/tuningfork/releases).

# Requirements
Unity version [2019.3 or later](https://unity3d.com/get-unity/download/archive)

# Getting started
## Import plugin

* Download ``tuningfork.unitypackage`` from the [releases page](https://github.com/android/tuningfork/releases).
* Select **Assets > Import Package > Custom Package**.
* Select the downloaded ``tuningfork.unitypackage``.
* In the Importing Package dialog, make sure that all package options are selected and click Import.

## Update plugin
Local changes to the Fidelity Parameters and Annotations are defined in ``DevTuningfork.cs``. That file is also included in the ``tuningfork.unitypackage`` file. To update the plugin, follow the Import steps above, but deselect ``DevTuningfork.cs`` to keep your local changes.

## Configure Tuningfork
Open **Android > Tuningfork** to configure the plugin.
See the full list of settings and parameters below.

# Settings
## Android Frame Pacing
To make the best of your game frame rate and to get precise measurements from Tuningfork it is recommended to turn on frame pacing optimization. Navigate to **[Player](https://docs.unity3d.com/Manual/class-PlayerSettingsAndroid.html) > Resolution and Presentation** and turn on **Optimized Frame Pacing**. Make sure that the option **Player > Quality > VSync Count** is on for each of your quality levels.
[The Android Frame Pacing](https://developer.android.com/games/sdk/frame-pacing) library has been integrated in Unity since 2019.3 and works for both OpenGL and Vulkan.

## API key
Create an API Key in the Cloud Console:

* Navigate to the [APIs & Services > Credentials](https://console.cloud.google.com/apis/credentials) panel in Cloud Console.
* Select **Create credentials**, then select API key from the dropdown menu.
* The API key created dialog box displays your newly created key.
* Select **Android Performance Parameters API** in API restrictions.

Once you’ve created your API key, copy it to the corresponding field in the plugin.

## Annotations
Annotations give contextual information about what your game is doing when a timing is recorded. For example:

* The current game scene
* A specific scene is loading
* A "big boss" is on the screen
* Any relevant game state information

A ``Scene`` enum is generated automatically once it detects changes in your build settings.
This ``Scene`` enum name is generated from the full path of your scene escaping any special symbols and spaces. ``Scene`` enum and ``LoadingState`` enum are always included.

Default annotations:

* Includes ``Scene`` and ``LoadingState``.
* Are set automatically when the active scene is changed. You don’t need to call ``SetAnnotation()`` yourself.
* ``LoadingState`` is always set to ``NotLoading``. In order to set when your game is in loading or not loading state, use custom annotations.

Custom annotations:

* You can create and include any annotation you think is suitable for your game.
* You can include ``Scene`` and ``LoadingState`` in your annotation.
* ``Scene scene`` and ``LoadingState loading_state`` are reserved fields, if added to your custom annotation it will be recognized as scene and loading state and analyzed accordingly. Note: ``Scene new_scene``/``LoadingState loading``/etc are not reserved fields.
* You need to call ``SetAnnotation(annotation)`` (see API section for more details) to apply an annotation during gameplay.

> Important:
Applying changes regenerates the annotation .cs file. Make sure to keep in sync annotations defined in the plugin and references to them in your code.

## Fidelity parameters
Fidelity parameters influence the performance and graphical fidelity of your game, such as mesh level-of-detail, texture resolution, and anti-aliasing method.

Default parameters:

* Unity quality levels are used as Tuningfork fidelity levels.
* The list of fidelity levels will be automatically generated from the Unity quality levels when you are exporting the android project or building an apk/appbundle.
* Unity QualitySettings of your game are updated automatically when they are updated from the server.

Custom parameters:

* You can create a number of different fidelity parameters to adjust your game performance. Example of fidelity parameters:
  * Quality level
  * Texture size
  * Shadow Resolution
  * etc
* You need to update your game settings yourself when new fidelity parameter changes are received from the server.

> Important:
Applying changes regenerates the fidelity parameters .cs file. Keep in sync fidelity parameters defined in the plugin and references to them in your code.

## Fidelity levels
Creation of fidelity levels is only available if you choose custom fidelity parameters. For default fidelity parameters these levels will be generated automatically.

* You can set your default fidelity level: select the necessary level and mark it as default .
* Trend icon will help you see how the value of your fidelity parameter is changing. An invalid trend is not-blocking and you still can use such levels, but it might be disallowed in future versions of the plugin.

## Advanced Instrumentation Settings
To customize data sent by Tuningfork you can modify instrumentation settings, e.g. you can change the submission method during the debug process to send upload requests more frequently. Only ``SYSCPU`` instrumentation key is available for the current version of Play Console. Tuningfork is using information about frame rate from the Android Frame Pacing library. If **Optimized Frame Pacing** is disabled or not available for the version of unity you are using, Tuningfork will use ``WaitForEndOfFrame`` instruction to collect information about frame rate. It will use the same instrumentation key (``SYSCPU``).

# Building the apk
If Tuningfork is enabled, when you export the android project or build the apk/appbundle, Tuningfork files will be copied under the **/assets/tuningfork/** folder.
It will contain:

* **dev_tuningfork.descriptor** file
* **tuningfork_settings.bin** file
* **dev_tuningfork_fidelityparams_N.bin** files which corresponds to quality levels in your game if you are using default fidelity parameters or to the custom fidelity levels you’ve created in the Fidelity levels section.

In the Editor all these files are located under **Google/Android/PerformanceParameters/Editor/AndroidAssets/**

# API
See the [Tuningfork Unity API References](https://github.com/android/tuningfork/tree/master/Google/Android/PerformanceParameters/Scripts/Tuningfork.cs).

# Terms & Conditions
By downloading the Tuningfork Unity Plugin, you agree to [Terms and Conditions](https://developer.android.com/studio/terms).
