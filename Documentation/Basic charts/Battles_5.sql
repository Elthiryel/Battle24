SELECT A.Name, SUM(A.AllStrength) AS TotalStrength FROM
(
	SELECT WA.Name, BE.ConflictSide, BE.AllStrength
	FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BE, WARS AS WA
	WHERE BA.ID = BE.BattleID AND WA.ID = BA.WarID
	GROUP BY WA.Name, BE.ConflictSide, BE.AllStrength
) AS A
GROUP BY A.Name order by TotalStrength DESC
