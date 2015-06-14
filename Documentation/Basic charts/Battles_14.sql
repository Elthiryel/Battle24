SELECT BattleYear, SUM(AllLosses) AS LossesSum FROM
(
	SELECT BA.Name, BB.ConflictSide, BB.AllLosses, YEAR(BA.StartDate) / 20 * 20 - 1000 AS BattleYear
	FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BB
	WHERE BA.ID = BB.BattleID AND BB.AllStrength > 0
	GROUP BY Ba.Name, BB.ConflictSide, BB.AllLosses, YEAR(BA.StartDate) / 20 * 20 - 1000
) AS A
GROUP BY BattleYear
ORDER BY BattleYear
