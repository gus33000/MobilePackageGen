# Mobile Package Gen - Create Mobile Packages from various image types

## Copyright

Copyright (c) 2024, Gustave Monce - gus33000.me - @gus33000

This software is released under the MIT license, for more information please see LICENSE.md

## Description

This tool enables creating CBS and SPKG packages out of:

- FFU Files
- VHDX Files (with OS Pools (x64 only))
- VHDX Files (without OS Pools)
- Mass Storage

## Usage

### Command

```batch
ToCBSFfuNoPool E:\DeviceImage\Flash.ffu E:\DeviceImage\FlashOutputCabs
```

### Command Output

```batch
Image To Component Based Servicing Cabinets tool
Version: 1.0.0.0

Getting Disks...
Getting Update OS Disks...

Found Disks:

LogFS 141ab2dd-a5ea-463f-900c-5a37f04c30e0 bc0330eb-3410-4951-a617-03898dbe3372 16777216 KnownFS
TZAPPS c50d8620-02c3-4255-975f-d4bd8581d6f3 14d11c40-2a3d-4f97-882d-103a1ec09333 16777216 KnownFS
PLAT 2b285af7-f3a9-4c53-be6e-b5d03f9b19a3 543c031a-4cb6-4897-bffe-4b485768a8ad 8388608 KnownFS
EFIESP 8183040a-8b44-4592-92f7-c6d9ee0560f7 ebd0a0a2-b9e5-4433-87c0-68b6b72699c7 33554432 KnownFS
MMOS 27a47557-8243-4c8e-9d30-846844c29c52 ebd0a0a2-b9e5-4433-87c0-68b6b72699c7 6291456 KnownFS
MainOS a76b8ce2-0187-4c13-8fca-8651c9b0620a ebd0a0a2-b9e5-4433-87c0-68b6b72699c7 3087007744 KnownFS
Data e2f90afc-9878-4a80-9362-fe8b588309fb ebd0a0a2-b9e5-4433-87c0-68b6b72699c7 14988345344 KnownFS
MainOS-UpdateOS-0 00000000-0000-0000-0000-000000000000 00000000-0000-0000-0000-000000000000 36440228 KnownFS

DPP b5ac5cfb-baf2-4037-bf3e-4ff70353fa81 ebd0a0a2-b9e5-4433-87c0-68b6b72699c7 8388608 UnknownFS
MODEM_FSG b57af4a7-b9be-4904-b485-098ada18e66f 638ff8e2-22c9-e33b-8f5d-0e81686a68cb 1572864 UnknownFS
MODEM_FS1 664f129f-7387-4a6d-a001-3284b2686458 ebbeadaf-22c9-e33b-8f5d-0e81686a68cb 1572864 UnknownFS
MODEM_FS2 4bd5a5bc-b24b-45a6-bee5-703245343a5a 0a288b1f-22c9-e33b-8f5d-0e81686a68cb 1572864 UnknownFS
MODEM_FSC 2133316f-0ff3-476f-95e7-36c4b1b7a3d2 57b90a16-22c9-e33b-8f5d-0e81686a68cb 16384 UnknownFS
DDR c3f002d3-7d3b-4eec-89be-6b9ae30583af 20a0c19c-286a-42fa-9ce7-f64c3226a794 1048576 UnknownFS
SEC 40547a50-4667-4348-9681-209ed5c9a688 303e6ac3-af15-4c54-9e9b-d9a8fbecf401 131072 UnknownFS
APDP 10b93a65-25b8-4d47-bd56-7024087012f4 e6e98da2-e22a-4d12-ab33-169e7deaa507 262144 UnknownFS
MSADP 38eaeecf-9ea4-49b2-b933-5a8716232ba6 ed9e8101-05fa-46b7-82aa-8d58770d200b 262144 UnknownFS
DPO 5f3da062-6900-41a9-b9da-d45804a97041 11406f35-1173-4869-807b-27df71802812 16384 UnknownFS
SSD f2d8f981-c5fb-4aee-a54e-2eb00e3d362e 2c86e742-745e-4fdd-bfd8-b6a7ac638772 16384 UnknownFS
UEFI_BS_NV 0cb8d846-6374-4c57-b740-ab42a09ec763 f0b4f48b-aeba-4ecf-9142-5dc30cdc3e77 262144 UnknownFS
UEFI_RT_NV bea9af93-1e20-4155-9248-7a5e7639503e 6bb94537-7d1c-44d0-9dfe-6d77c011dbfc 262144 UnknownFS
LIMITS 71a39428-15f3-4500-ad2e-d6a4827b4498 10a0c19c-516a-5444-5ce3-664c3226a794 16384 UnknownFS
SBL1 73a18223-2924-4ee7-aaee-9bfcb894ef2a dea0ba2c-cbdd-4805-b4f9-f428251c3e98 1048576 UnknownFS
PMIC dc4afb4d-ea54-4afd-b37f-f02efd024812 c00eef24-7709-43d6-9799-dd2b411e7a3c 524288 UnknownFS
DBI 9ddbf00e-0d09-4216-a875-3ac12adf8be1 d4e0d938-b7fa-48c1-9d21-bc5ed5c4b203 49152 UnknownFS
UEFI f8e845a1-4ac8-46e3-bdde-86649f22e90a 400ffdcd-22e0-47e7-9a23-f16ed9382388 2097152 UnknownFS
RPM bf39a435-169c-4473-8f8b-26b908a290c9 098df793-d712-413d-9d4e-89d711772228 512000 UnknownFS
TZ 3a89aded-07a0-4a35-bdc0-7d886c151cf6 a053aa7f-40b8-4b1c-ba08-2f68ac71a4f4 1048576 UnknownFS
HYP c78b5309-dd91-4dde-86fb-842931cff2c9 e1a6a689-0c8d-4cc6-b4e8-55a4320fbd8a 512000 UnknownFS
WINSECAPP ae1f7414-22c2-4cc4-afa1-37560374e987 69b4201f-a5ad-45eb-9f49-45b38ccdaef5 524288 UnknownFS
BACKUP_SBL1 2846959b-301f-4bd1-a594-9c89458532f0 a3381699-350c-465e-bd5d-fa3ab901a39a 1048576 UnknownFS
BACKUP_PMIC b3eb1bfa-b10d-43a9-b4cd-8281834cc6aa a3381699-350c-465e-bd5d-fa3ab901a39a 524288 UnknownFS
BACKUP_DBI a9edf692-1360-40d3-818f-7469e0d6faa7 a3381699-350c-465e-bd5d-fa3ab901a39a 49152 UnknownFS
BACKUP_UEFI 05870b82-bace-49f8-a893-502ae46c4fdc a3381699-350c-465e-bd5d-fa3ab901a39a 2097152 UnknownFS
BACKUP_RPM 57905308-40cd-4778-8dde-33562dde0f3c a3381699-350c-465e-bd5d-fa3ab901a39a 512000 UnknownFS
BACKUP_TZ 5ef8e832-81a6-430a-8dee-a0f0abbbe5a5 a3381699-350c-465e-bd5d-fa3ab901a39a 1048576 UnknownFS
BACKUP_HYP b7e9aad3-c1db-4369-ba9c-769cbbb0d0e6 a3381699-350c-465e-bd5d-fa3ab901a39a 512000 UnknownFS
BACKUP_WINSECAPP 8d1e2897-f576-4645-9dc0-8b845d6b08ca a3381699-350c-465e-bd5d-fa3ab901a39a 524288 UnknownFS
BACKUP_TZAPPS 22a5b95c-9d0a-4977-b1dc-43122f0d202b a3381699-350c-465e-bd5d-fa3ab901a39a 16777216 UnknownFS

Building CBS Cabinet Files...

Processing 139 of 257 - Creating package MainOS\Microsoft.MobileCore.Prod.MainOS~31bf3856ad364e35~arm~~
[============================74%============               ]
```

### Result

```batch
C:.
├───EFIESP
│       Microsoft.EFIESP.Production~31bf3856ad364e35~arm~~.cab
│       Microsoft.MobileCore.Prod.EFIESP~31bf3856ad364e35~arm~~.cab
│       Microsoft.MS_BOOTSEQUENCE_RETAIL.EFIESP~31bf3856ad364e35~arm~~.cab
│       Microsoft.RELEASE_PRODUCTION.EFIESP~31bf3856ad364e35~arm~~.cab
│       MMO.BASE.Phone.EFIESP~31bf3856ad364e35~arm~~.cab
│       MMO.BASE.Variant.EFIESP~31bf3856ad364e35~arm~~.cab
│       MMO.CITYMAN_LTE_ROW_DSDS.Customizations.EFIESP~31bf3856ad364e35~arm~~.cab
│       MMO.DEVICE_CITYMAN_LTE_ROW_DSDS.Phone.EFIESP~31bf3856ad364e35~arm~~.cab
│       MMO.SOC_QC8994.Phone.EFIESP~31bf3856ad364e35~arm~~.cab
│
├───MainOS
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~af-za~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~ar-sa~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~az-latn-az~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~bg-bg~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~bn-bd~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~ca-es~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~cs-cz~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~da-dk~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~de-de~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~el-gr~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~en-gb~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~en-in~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~en-us~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~es-es~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~es-mx~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~et-ee~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~eu-es~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~fa-ir~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~fi-fi~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~fr-ca~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~fr-ch~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~fr-fr~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~gl-es~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~ha-latn-ng~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~hi-in~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~hr-hr~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~hu-hu~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~hy-am~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~id-id~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~it-it~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~ja-jp~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~kk-kz~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~ko-kr~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~lt-lt~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~lv-lv~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~mk-mk~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~ms-my~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~nb-no~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~nl-be~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~nl-nl~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~pl-pl~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~pt-br~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~pt-pt~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~ro-ro~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~ru-ru~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~sk-sk~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~sl-si~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~sq-al~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~sr-latn-rs~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~sv-se~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~sw-ke~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~tr-tr~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~uk-ua~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~uz-latn-uz~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~vi-vn~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~zh-cn~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~zh-hk~.cab
│       Microsoft.Input.mtf_LANG~31bf3856ad364e35~arm~zh-tw~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~af-za~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~am-et~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~ar-sa~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~az-latn-az~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~be-by~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~bg-bg~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~bn-bd~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~ca-es~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~cs-cz~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~da-dk~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~de-de~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~el-gr~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~en-gb~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~en-us~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~es-es~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~es-mx~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~et-ee~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~eu-es~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~fa-ir~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~fi-fi~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~fil-ph~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~fr-ca~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~fr-fr~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~gl-es~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~ha-latn-ng~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~he-il~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~hi-in~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~hr-hr~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~hu-hu~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~id-id~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~is-is~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~it-it~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~ja-jp~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~kk-kz~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~km-kh~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~kn-in~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~ko-kr~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~lo-la~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~lt-lt~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~lv-lv~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~mk-mk~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~ml-in~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~ms-my~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~nb-no~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~nl-nl~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~pl-pl~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~pt-br~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~pt-pt~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~ro-ro~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~ru-ru~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~sk-sk~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~sl-si~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~sq-al~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~sr-latn-rs~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~sv-se~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~sw-ke~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~ta-in~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~te-in~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~th-th~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~tr-tr~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~uk-ua~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~uz-latn-uz~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~vi-vn~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~zh-cn~.cab
│       Microsoft.MainOS.Production_LANG~31bf3856ad364e35~arm~zh-tw~.cab
│       Microsoft.MainOS.Production_RES_1440x2560~31bf3856ad364e35~arm~~.cab
│       Microsoft.MainOS.Production~31bf3856ad364e35~arm~~.cab
│
└───PLAT
        MMO.BASE.Phone.PLAT~31bf3856ad364e35~arm~~.cab
        MMO.BASE.Variant.PLAT~31bf3856ad364e35~arm~~.cab
        MMO.DEVICE_CITYMAN_LTE_ROW_DSDS.Phone.PLAT~31bf3856ad364e35~arm~~.cab
        MMO.SOC_QC8994.Phone.PLAT~31bf3856ad364e35~arm~~.cab
```

## Demo
