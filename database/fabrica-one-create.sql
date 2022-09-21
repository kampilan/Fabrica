# ---------------------------------------------------------------------- #
# Script generated with: DeZign for Databases 12.4.2                     #
# Target DBMS:           MySQL 8                                         #
# Project file:          fabrica-one.dez                                 #
# Project name:                                                          #
# Author:                                                                #
# Script type:           Database creation script                        #
# Created on:            2022-09-21 15:44                                #
# ---------------------------------------------------------------------- #


# ---------------------------------------------------------------------- #
# Add tables                                                             #
# ---------------------------------------------------------------------- #

# ---------------------------------------------------------------------- #
# Add table "WorkTopics"                                                 #
# ---------------------------------------------------------------------- #



CREATE TABLE `WorkTopics` (
    `Id` BIGINT NOT NULL AUTO_INCREMENT,
    `Uid` VARCHAR(40) NOT NULL DEFAULT '',
    `Environment` VARCHAR(100) NOT NULL DEFAULT '',
    `TenantUid` VARCHAR(40) NOT NULL DEFAULT '',
    `Topic` VARCHAR(255) NOT NULL DEFAULT '',
    `Synchronous` BOOL NOT NULL DEFAULT 0,
    `ClientName` VARCHAR(100) NOT NULL DEFAULT '',
    `Path` VARCHAR(255) NOT NULL DEFAULT '',
    `FullUrl` VARCHAR(255) NOT NULL DEFAULT '',
    CONSTRAINT `PK_WorkTopics` PRIMARY KEY (`Id`)
);



CREATE INDEX `IDX_WorkTopics_1` ON `WorkTopics` (`Uid`);

CREATE INDEX `IDX_WorkTopics_2` ON `WorkTopics` (`TenantUid`,`Environment`,`Topic`);
