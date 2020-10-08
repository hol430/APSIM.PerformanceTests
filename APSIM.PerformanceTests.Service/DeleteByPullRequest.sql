--BEGIN TRANSACTION

--Look at the Data
--SELECT distinct [PullRequestId], [RunDate], [StatsAccepted]
--	FROM [APSIM.PerformanceTests].[dbo].[ApsimFiles]
--	ORDER BY [PullRequestId], [RunDate]
--Sample Usage
--EXEC usp_DeleteByPullRequestId 1996
	
--FOR MANUAL DELETION OF RECORDS, COMMENT OUT EVERYTHING ABOVE THIS LINE
--==================================================================================================
-- AND UNCOMMENT THE FOLLOWING TWO LINES:
--DECLARE @PullRequestID INT
--SET @PullRequestID = 2093
	
--SELECT * FROM [dbo].[PredictedObservedTests]
--WHERE [AcceptedPredictedObservedTestsID] = 131997
--remove any foreign key references

UPDATE PredictedObservedTests
SET AcceptedPredictedObservedTestsID = NULL
WHERE AcceptedPredictedObservedTestsID IN (
	SELECT pot.ID
	FROM PredictedObservedTests pot
	WHERE pot.PredictedObservedDetailsID IN (
		SELECT pod.ID
		FROM PredictedObservedDetails pod
		INNER JOIN ApsimFiles a ON a.ID = pod.ApsimFilesID
		WHERE a.PullRequestId = @PullRequestID
	)
);	        

----delete any tests data
DELETE FROM PredictedObservedTests
WHERE PredictedObservedDetailsID IN (
	SELECT p.ID
	FROM PredictedObservedDetails p
	INNER JOIN ApsimFiles a ON a.ID = p.ApsimFilesID
	WHERE a.PullRequestId = @PullRequestID
);

----delete any predicted observed values
DELETE FROM PredictedObservedValues
WHERE PredictedObservedDetailsID IN (
	SELECT p.ID
	FROM PredictedObservedDetails p
	INNER JOIN ApsimFiles a ON a.ID = p.ApsimFilesID
	WHERE a.PullRequestId = @PullRequestID
);

----delete any foreign key refrences in predicted observed details table
UPDATE PredictedObservedDetails
SET AcceptedPredictedObservedDetailsID = NULL
WHERE AcceptedPredictedObservedDetailsID IN (
	SELECT p.ID
	FROM PredictedObservedDetails p
	INNER JOIN ApsimFiles a ON a.ID = p.ApsimFilesID
	WHERE a.PullRequestId = @PullRequestID
);

----delete any predicted obvserved details table
DELETE FROM PredictedObservedDetails
WHERE ApsimFilesID IN (
	SELECT ID FROM ApsimFiles 
	WHERE PullRequestId = @PullRequestID
);

--delete any simulation data
DELETE FROM Simulations
WHERE ApsimFilesID IN (
	SELECT ID FROM ApsimFiles 
	WHERE PullRequestId = @PullRequestID
);

--and finally, delete the apsim file information
DELETE FROM ApsimFiles 
WHERE PullRequestId = @PullRequestID;

--now update the acceptStatsLog 
UPDATE AcceptStatsLogs
SET LogStatus = 0,
	LogReason = LogReason + ' - DELETED.' 
WHERE PullRequestId = @PullRequestID;

-- If the obove works, then commit the transaction.
--COMMIT TRANSACTION;
