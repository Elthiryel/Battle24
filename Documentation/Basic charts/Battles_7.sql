SELECT Name, COUNT(*) AS Wins FROM
(
	SELECT BE.CountryGroup AS Name, BB.BattleID AS BattleID, COUNT(*) AS Cnt
	FROM BELLIGERENTS AS BE, BATTLES_BELLIGERENTS AS BB
	WHERE BE.ID = BB.BelligerentID AND (Result = 'Win' OR Result = 'Decisive Win')
	GROUP BY BE.CountryGroup, BB.BattleID
) AS A
GROUP BY Name
ORDER BY Wins DESC
