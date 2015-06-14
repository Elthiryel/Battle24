SELECT CASE WHEN StrongerLosses > WeakerLosses THEN 'StrongerHasMoreLosses' ELSE 'StrongerDoesNotHaveMoreLosses' END, COUNT(*) AS ResultCount FROM
(
	SELECT LeftResult AS Result, LeftLosses AS StrongerLosses, RightLosses AS WeakerLosses FROM
	(
		SELECT C.Name, C.AllStrength AS LeftStrength, D.AllStrength AS RightStrength, C.Result AS LeftResult, D.Result AS RightResult, C.AllLosses AS LeftLosses, D.AllLosses AS RightLosses FROM
		(
			SELECT Name, ConflictSide, Result, AllLosses, SUM(AllStrength) AS AllStrength FROM
			(
				SELECT BA.Name, BB.ConflictSide, BB.AllStrength, BB.AllLosses, BB.Result
				FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BB
				WHERE BA.ID = BB.BattleID AND ConflictSide = 0
				GROUP BY BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result, BB.AllLosses
			) AS A
			GROUP BY Name, ConflictSide, Result, AllLosses
		) AS C,
		(
			SELECT Name, ConflictSide, Result, AllLosses, SUM(AllStrength) AS AllStrength FROM
			(
				SELECT BA.Name, BB.ConflictSide, BB.AllStrength, BB.AllLosses, BB.Result
				FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BB
				WHERE BA.ID = BB.BattleID AND ConflictSide = 1
				GROUP BY BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result, BB.AllLosses
			) AS B
			GROUP BY Name, ConflictSide, Result, AllLosses
		) AS D
		WHERE C.Name = D.Name
	) AS E
	WHERE LeftStrength > RightStrength
	UNION ALL
	SELECT RightResult AS Result, RightLosses AS StrongerLosses, LeftLosses AS WeakerLosses FROM
	(
		SELECT C.Name, C.AllStrength AS LeftStrength, D.AllStrength AS RightStrength, C.Result AS LeftResult, D.Result AS RightResult, C.AllLosses AS LeftLosses, D.AllLosses AS RightLosses FROM
		(
			SELECT Name, ConflictSide, Result, AllLosses, SUM(AllStrength) AS AllStrength FROM
			(
				SELECT BA.Name, BB.ConflictSide, BB.AllStrength, BB.AllLosses, BB.Result
				FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BB
				WHERE BA.ID = BB.BattleID AND ConflictSide = 0
				GROUP BY BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result, BB.AllLosses
			) AS A
			GROUP BY Name, ConflictSide, Result, AllLosses
		) AS C,
		(
			SELECT Name, ConflictSide, Result, AllLosses, SUM(AllStrength) AS AllStrength FROM
			(
				SELECT BA.Name, BB.ConflictSide, BB.AllStrength, BB.AllLosses, BB.Result
				FROM BATTLES AS BA, BATTLES_BELLIGERENTS AS BB
				WHERE BA.ID = BB.BattleID AND ConflictSide = 1
				GROUP BY BA.Name, BB.ConflictSide, BB.AllStrength, BB.Result, BB.AllLosses
			) AS B
			GROUP BY Name, ConflictSide, Result, AllLosses
		) AS D
		WHERE C.Name = D.Name
	) AS E
	WHERE LeftStrength < RightStrength
) AS F
GROUP BY CASE WHEN StrongerLosses > WeakerLosses THEN 'StrongerHasMoreLosses' ELSE 'StrongerDoesNotHaveMoreLosses' END
