# ---------------------------------------------------------------------- #
# Script generated with: DeZign for Databases 12.4.2                     #
# Target DBMS:           MySQL 8                                         #
# Project file:          fabrica-one.dez                                 #
# Project name:                                                          #
# Author:                                                                #
# Script type:           Database drop script                            #
# Created on:            2022-09-21 15:44                                #
# ---------------------------------------------------------------------- #


# ---------------------------------------------------------------------- #
# Drop table "WorkTopics"                                                #
# ---------------------------------------------------------------------- #

# Remove autoinc for PK drop #

ALTER TABLE `WorkTopics` MODIFY `Id` BIGINT NOT NULL;

# Drop constraints #

ALTER TABLE `WorkTopics` DROP PRIMARY KEY;

DROP TABLE `WorkTopics`;
