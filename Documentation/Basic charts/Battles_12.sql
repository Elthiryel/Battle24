SELECT YEAR(StartDate) / 20 * 20 - 1000 AS BattleYear, COUNT(*) AS BattleCount
FROM BATTLES
WHERE StartDate IS NOT NULL
GROUP BY YEAR(StartDate) / 20 * 20 - 1000
ORDER BY BattleYear
