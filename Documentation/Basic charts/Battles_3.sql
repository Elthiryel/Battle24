SELECT BA.Name, YEAR(BA.StartDate)-1000 as 'Year', BE.ConflictSide, BE.AllStrength, BE.AllLosses, CONVERT(DECIMAL, BE.AllLosses) / CONVERT(DECIMAL, BE.AllStrength) AS Ratio
FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BE
WHERE BA.ID = BE.BattleID AND BE.AllStrength > 0 AND BE.AllLosses > 0
GROUP BY BA.Name, BE.ConflictSide, BE.AllStrength, BE.AllLosses,YEAR(BA.StartDate)-1000
ORDER BY Ratio DESC
