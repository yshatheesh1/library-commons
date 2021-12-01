CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;

CREATE TABLE `Actions` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `ActionId` varbinary(16) NOT NULL,
    `Name` varchar(250) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `ScheduleSites` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `ScheduleSiteId` varbinary(16) NOT NULL,
    `Name` varchar(200) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `States` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `StateId` varbinary(16) NOT NULL,
    `Name` text NULL,
    PRIMARY KEY (`Id`)
);

CREATE TABLE `Fingerprint` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `FingerprintId` varbinary(16) NOT NULL,
    `NmlsId` bigint NOT NULL,
    `StateId` bigint NOT NULL,
    `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP(),
    `UpdatedDate` datetime NOT NULL,
    `CreatedById` bigint NOT NULL,
    `LastUpdatedById` bigint NOT NULL,
    `ExpirationDate` datetime NULL,
    `RenewalDate` datetime NULL,
    `IsActive` tinyint(1) NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Fingerprint_States_StateId` FOREIGN KEY (`StateId`) REFERENCES `States` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Schedules` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `ScheduleId` varbinary(16) NOT NULL,
    `FingerPrintId` bigint NOT NULL,
    `ScheduleDate` datetime NOT NULL,
    `ScheduleSiteId` bigint NOT NULL,
    `ActionId` bigint NOT NULL,
    `CreatedDate` datetime NOT NULL,
    `CreatedById` bigint NOT NULL,
    `LastUpdatedDate` datetime NOT NULL,
    `LastUpdatedById` bigint NOT NULL,
    PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Schedules_Actions_ActionId` FOREIGN KEY (`ActionId`) REFERENCES `Actions` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Schedules_Fingerprint_FingerPrintId` FOREIGN KEY (`FingerPrintId`) REFERENCES `Fingerprint` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Schedules_ScheduleSites_ScheduleSiteId` FOREIGN KEY (`ScheduleSiteId`) REFERENCES `ScheduleSites` (`Id`) ON DELETE CASCADE
);

CREATE UNIQUE INDEX `IX_Actions_ActionId` ON `Actions` (`ActionId`);

CREATE UNIQUE INDEX `IX_Fingerprint_FingerprintId` ON `Fingerprint` (`FingerprintId`);

CREATE INDEX `IX_Fingerprint_StateId` ON `Fingerprint` (`StateId`);

CREATE INDEX `IX_Schedules_ActionId` ON `Schedules` (`ActionId`);

CREATE INDEX `IX_Schedules_FingerPrintId` ON `Schedules` (`FingerPrintId`);

CREATE UNIQUE INDEX `IX_Schedules_ScheduleId` ON `Schedules` (`ScheduleId`);

CREATE INDEX `IX_Schedules_ScheduleSiteId` ON `Schedules` (`ScheduleSiteId`);

CREATE UNIQUE INDEX `IX_ScheduleSites_ScheduleSiteId` ON `ScheduleSites` (`ScheduleSiteId`);

CREATE UNIQUE INDEX `IX_States_StateId` ON `States` (`StateId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20211201045146_initial', '5.0.10');

COMMIT;

START TRANSACTION;

ALTER TABLE `Schedules` DROP CONSTRAINT `FK_Schedules_Actions_ActionId`;

ALTER TABLE `Schedules` MODIFY `ActionId` bigint NULL;

ALTER TABLE `Schedules` ADD CONSTRAINT `FK_Schedules_Actions_ActionId` FOREIGN KEY (`ActionId`) REFERENCES `Actions` (`Id`) ON DELETE CASCADE;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20211201061537_schedule_actionidasnull', '5.0.10');

COMMIT;

