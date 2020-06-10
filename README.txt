===============================================================================
DEVICE CONFIGURATION TOOL REVISON HISTORY
===============================================================================

-------------------------------------------------------------------------------
VERSION.032 (06/10/2020)

I. Unattended using the Ux 301/401/100 (EMV L2 Version 7.0.3l)
   Terminal 25 – Config 8C - CHECKSUM=D196BA9D 

II. Attended using the Engage P200 (EMV L2 Version 7.0.3r)
    Terminal 22 – Config 1C - CHECKSUM=96369E1F
-------------------------------------------------------------------------------

1. CONTLEMV.CFG
   - ENGAGE/UX301 SPECIFIC CONFIGURATIONS
   
2. ICCDATA.DAT
   - ENGAGE/UX301 SPECIFIC CONFIGURATIONS

3. ICCKEYS.KEY
   - ENGAGE/UX301 SPECIFIC CONFIGURATIONS
    
4. Added EMV Kernel checksum validation
   - ENGAGE/UX301 SPECIFIC CONFIGURATIONS

-------------------------------------------------------------------------------
VERSION.031 (06/05/2020)
-------------------------------------------------------------------------------
1. CONTLEMV.CFG
   - Changed TAG 9F35 from '22' to '25'
   - Changed TAG 9F33 from '60 08 08' to 'E0 48 C8'
   
2. ICCDATA.DAT
   - Changed TAG 9F35 from '22' to '25'
   - Changed TAG 9F33 from 'E048C8' to 'E0B8C8'

3. ICCKEYS.KEY
   - Changed TAG 9F35 from '22' to '25'
   - Changed TAG 9F33 from 'E048C8' to 'E0B8C8'
    
4. Added EMV Kernel checksum validation

5. Added CONFIGURATION check for STATUS selection

6. Added Firmware version to STATUS report

-------------------------------------------------------------------------------
VERSION.030 (06/03/2020)
-------------------------------------------------------------------------------
1. Added a000000384.c1
2. Added cicapp.cfg
3. Added mapp.cfg

-------------------------------------------------------------------------------
VERSION.027 (06/02/2020
-------------------------------------------------------------------------------
1. Initial release with unvalidated config files