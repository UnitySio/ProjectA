-- --------------------------------------------------------
-- 호스트:                          wizard87.com
-- 서버 버전:                        10.3.24-MariaDB - Source distribution
-- 서버 OS:                        Linux
-- HeidiSQL 버전:                  11.3.0.6295
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- siogames_main 데이터베이스 구조 내보내기
DROP DATABASE IF EXISTS `siogames_main`;
CREATE DATABASE IF NOT EXISTS `siogames_main` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE `siogames_main`;

-- 테이블 siogames_main.account_info 구조 내보내기
DROP TABLE IF EXISTS `account_info`;
CREATE TABLE IF NOT EXISTS `account_info` (
  `account_unique_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `account_authLv` tinyint(3) unsigned NOT NULL DEFAULT 0,
  `account_email` varchar(100) DEFAULT NULL,
  `account_password` varchar(256) DEFAULT NULL,
  `account_guest_token` varchar(50) DEFAULT NULL,
  `account_oauth_token_google` varchar(50) DEFAULT NULL,
  `account_oauth_token_apple` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`account_unique_id`),
  UNIQUE KEY `account_email` (`account_email`),
  UNIQUE KEY `account_oauth_token_google` (`account_oauth_token_google`),
  UNIQUE KEY `account_oauth_token_apple` (`account_oauth_token_apple`),
  UNIQUE KEY `account_guest_token` (`account_guest_token`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- 테이블 데이터 siogames_main.account_info:~0 rows (대략적) 내보내기
DELETE FROM `account_info`;
/*!40000 ALTER TABLE `account_info` DISABLE KEYS */;
/*!40000 ALTER TABLE `account_info` ENABLE KEYS */;

-- 테이블 siogames_main.character_info 구조 내보내기
DROP TABLE IF EXISTS `character_info`;
CREATE TABLE IF NOT EXISTS `character_info` (
  `character_unique_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `account_unique_id` int(10) unsigned NOT NULL,
  `character_name` varchar(15) DEFAULT NULL,
  `character_Lv` int(10) unsigned NOT NULL DEFAULT 0,
  `character_exp` int(10) unsigned NOT NULL DEFAULT 0,
  `character_hp` int(10) unsigned NOT NULL DEFAULT 0,
  `character_atk` int(10) unsigned NOT NULL DEFAULT 0,
  `character_def` int(10) unsigned NOT NULL DEFAULT 0,
  `character_spec_stat` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`character_unique_id`),
  KEY `account_unique_id_character` (`account_unique_id`),
  CONSTRAINT `account_unique_id_character` FOREIGN KEY (`account_unique_id`) REFERENCES `account_info` (`account_unique_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- 테이블 데이터 siogames_main.character_info:~0 rows (대략적) 내보내기
DELETE FROM `character_info`;
/*!40000 ALTER TABLE `character_info` DISABLE KEYS */;
/*!40000 ALTER TABLE `character_info` ENABLE KEYS */;

-- 테이블 siogames_main.player_info 구조 내보내기
DROP TABLE IF EXISTS `player_info`;
CREATE TABLE IF NOT EXISTS `player_info` (
  `player_unique_id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `account_unique_id` int(10) unsigned NOT NULL,
  `player_Lv` int(10) unsigned NOT NULL DEFAULT 0,
  `player_exp` int(10) unsigned NOT NULL DEFAULT 0,
  `player_stamina` int(10) unsigned NOT NULL DEFAULT 0,
  `player_ingame_point` int(10) unsigned NOT NULL DEFAULT 0,
  `player_cash_point` int(10) unsigned NOT NULL DEFAULT 0,
  `player_nickname` varchar(15) DEFAULT NULL,
  `player_profile_image_url` varchar(256) DEFAULT NULL,
  `player_gender` varchar(5) DEFAULT NULL,
  `player_birthdate` varchar(11) DEFAULT NULL,
  PRIMARY KEY (`player_unique_id`),
  UNIQUE KEY `account_unique_id` (`account_unique_id`),
  CONSTRAINT `account_unique_id_player` FOREIGN KEY (`account_unique_id`) REFERENCES `account_info` (`account_unique_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- 테이블 데이터 siogames_main.player_info:~0 rows (대략적) 내보내기
DELETE FROM `player_info`;
/*!40000 ALTER TABLE `player_info` DISABLE KEYS */;
/*!40000 ALTER TABLE `player_info` ENABLE KEYS */;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
