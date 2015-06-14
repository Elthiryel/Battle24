SELECT A.Name, COUNT(*) AS BattleCount FROM
(
	SELECT BE.CountryGroup AS Name, BB.BattleID AS BattleID, COUNT(*) AS Cnt
	FROM BELLIGERENTS AS BE, BATTLES_BELLIGERENTS AS BB
	WHERE BE.ID = BB.BelligerentID
	GROUP BY BE.CountryGroup, BB.BattleID
) AS A
GROUP BY A.Name
ORDER BY BattleCount DESC
