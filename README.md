# README #

This an application to load HMAC KEYS.

### What is this repository for? ###

* KEY_LOADER APPLICATION
* 1.00.0.(100)
* git remote add origin https://github.com/web-projects/KEY_LOADER.git

### How do I get set up? ###

* Summary of set up
* Configuration
* Dependencies
* Database configuration
* How to run tests
* Deployment instructions

### Contribution guidelines ###

* Writing tests
* Code review
* Other guidelines

### GIT NOTES ###

*  AUTO-CONVERTING CRLF line endings into LF
   $ git config --global core.autocrlf true
   
### HISTORY ###

* 20200520 - Initial repository
* 20200521 - Added HMAC Validator
* 20200522 - Added HMAC Loader
           - Added ADE Active Key Slot Reporting
           - Added Configs Updater
* 20200525 - Enhanced Configs Updater
* 20200526 - Added Feature Enablement Token to Updater
           - Corrected Key Generation
* 20200527 - Updated locking configs
* 20200528 - Added Device Configuration
* 20200529 - Moved Assets to resources
* 20200530 - Added Slot-0 and Slot-8 configuration updates
           - Added missing configuration files
* 20200602 - Configs update
* 20200603 - Target Build 0.27
* 20200604 - Target Build 0.30
* 20200605 - Target Build 0.31
* 20200606 - Added EMV Kernel Checksum validation
* 20200608 - Added CONFIGURATION validation to STATUS
* 20200609 - Added Firmware version to STATUS.
* 20200610 - Added ENGAGE/UX301 device specific configurations
* 20200611 - Added Templates for TAG processing
* 20200618 - Fixed mapp.cfg file size/hash change when rebooted
* 20200620 - Target Build 0.32
* 20200625 - Target Build 0.33
* 20200710 - Target Build 0.34
* 20200714 - Added TDOL.CFG (TAG 5F20 PROCESSING)
* 20200716 - Target Build 0.34.1
* 20200723 - Target Build 0.35.00
* 20200918 - Target Build 0.39.00
* 20201008 - Added HMAC changes to new secrets.
* 20201016 - Fixes to unlock mechanism.
* 20210115 - Improved initialization workflow
* 20210125 - ONLINE PIN IPP implementation
* 20210204 - Multi-Device Selection via app settings
* 20210208 - Configuration via Package
* 20210211 - Added M400 device
* 20210225 - Corrected IPP key reading slot
* 20210325 - V0 EMV Signed Configuration Packages
* 20210408 - Added idle screen update for Raptor/Engage devices
* 20210419 - V1 EMV Signed Configuration Packages 
* 20210503 - Fixed TAG 9F0D reporting
* 20210518 - Target Build 0.47
* 20210520 - Added device signed option: Sphere and Verifone-DEV
* 20210526 - Added VIPA restart command
