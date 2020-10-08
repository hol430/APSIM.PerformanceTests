--BEGIN TRANSACTION

--SELECT * FROM [dbo].[PredictedObservedTests]
--WHERE [AcceptedPredictedObservedTestsID] = 131997
--remove any foreign key references 
UPDATE PredictedObservedTests
SET AcceptedPredictedObservedTestsID = NULL
WHERE AcceptedPredictedObservedTestsID IN (
	SELECT ID
		FROM PredictedObservedTests
		WHERE PredictedObservedDetailsID IN 
		(SELECT ID FROM PredictedObservedDetails WHERE ApsimFilesID IN 
			(SELECT ID FROM ApsimFiles
				WHERE PullRequestId = @PullRequestID
				AND RunDate != @RunDate))
				);

--delete any tests data
DELETE FROM PredictedObservedTests
WHERE PredictedObservedDetailsID IN 
	(SELECT ID FROM PredictedObservedDetails WHERE ApsimFilesID IN 
		(SELECT ID FROM ApsimFiles
			WHERE PullRequestId = @PullRequestID
			AND RunDate != @RunDate));
	 
--delete any predicted observed values
DELETE FROM PredictedObservedValues
WHERE PredictedObservedDetailsID IN 
	(SELECT ID FROM PredictedObservedDetails WHERE ApsimFilesID IN 
		(SELECT ID FROM ApsimFiles
			WHERE PullRequestId = @PullRequestID
			AND RunDate != @RunDate));
	 
--delete any foreign key refrences
UPDATE PredictedObservedDetails
SET AcceptedPredictedObservedDetailsID = NULL
WHERE AcceptedPredictedObservedDetailsID IN 
	(SELECT ID
		FROM PredictedObservedDetails
		WHERE ApsimFilesID IN 
		(SELECT ID FROM ApsimFiles
			WHERE PullRequestId = @PullRequestID
			AND RunDate != @RunDate));

--delete any predicted obvserved table details
DELETE FROM PredictedObservedDetails
WHERE ApsimFilesID IN 
		(SELECT ID FROM ApsimFiles
			WHERE PullRequestId = @PullRequestID
			AND RunDate != @RunDate);

--delete any simulation data
DELETE FROM Simulations
WHERE ApsimFilesID IN 
		(SELECT ID FROM ApsimFiles
			WHERE PullRequestId = @PullRequestID
			AND RunDate != @RunDate);

----and finally, delete the apsim file information
DELETE FROM ApsimFiles
	WHERE PullRequestId = @PullRequestID 
	AND RunDate != @RunDate;
		
--this only allows for one PullRequestID 'set' at a time,
-- doesn't include Run Date
UPDATE AcceptStatsLogs
SET LogStatus = 0,
	LogReason = LogReason + ' - DELETED.' 
WHERE PullRequestId = @PullRequestID
AND LogReason NOT LIKE '%DELETED%'; 

-- If the obove works, then commit the transaction.
--COMMIT TRANSACTION;