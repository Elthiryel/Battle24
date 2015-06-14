SELECT A.Name,YEAR(A.StartDate)-1000 as 'Year', SUM(A.AllStrength) AS TotalStrength FROM
(
	SELECT BA.Name, BE.ConflictSide, BE.AllStrength, BA.StartDate
	FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BE
	WHERE BA.ID = BE.BattleID
	GROUP BY BA.Name, BE.ConflictSide, BE.AllStrength, BA.StartDate
) AS A
GROUP BY A.Name,YEAR(A.StartDate) - 1000
ORDER By TotalStrength DESC
