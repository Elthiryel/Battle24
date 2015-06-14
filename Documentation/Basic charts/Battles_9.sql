SELECT A.Name, A.Wins, B.AllBattles, CONVERT(Decimal, A.Wins) / CONVERT(Decimal, B.AllBattles) AS Ratio FROM
(
	SELECT Name, COUNT(*) AS Wins FROM
	(
		SELECT BE.CountryGroup AS Name, BB.BattleID AS BattleID, COUNT(*) AS Cnt
		FROM BELLIGERENTS AS BE, BATTLES_BELLIGERENTS AS BB
		WHERE BE.ID = BB.BelligerentID AND (Result = 'Win' OR Result = 'Decisive Win')
		GROUP BY BE.CountryGroup, BB.BattleID
	) AS A
	GROUP BY Name
) AS A,
(
	SELECT Name, COUNT(*) AS AllBattles FROM
	(
		SELECT BE.CountryGroup AS Name, BB.BattleID AS BattleID, COUNT(*) AS Cnt
		FROM BELLIGERENTS AS BE, BATTLES_BELLIGERENTS AS BB
		WHERE BE.ID = BB.BelligerentID AND Result != 'Inconclusive'
		GROUP BY BE.CountryGroup, BB.BattleID
	) AS A
	GROUP BY Name
) AS B
WHERE A.Name = B.Name AND B.AllBattles >= 10
ORDER BY Ratio DESC
