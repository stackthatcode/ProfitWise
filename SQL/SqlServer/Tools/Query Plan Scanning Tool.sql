USE ProfitWise
GO

CREATE FUNCTION ScanInCacheFromDatabase 
(     
		-- Add the parameters for the function here
		@DatabaseName varchar(50)
)
RETURNS TABLE 
AS
RETURN 
(
with XMLNAMESPACES
(default 'http://schemas.microsoft.com/sqlserver/2004/07/showplan')
select qp.query_plan,qt.text, 
statement_start_offset, statement_end_offset,
creation_time, last_execution_time,
execution_count, total_worker_time,
last_worker_time, min_worker_time,
max_worker_time, total_physical_reads,
last_physical_reads, min_physical_reads,
max_physical_reads, total_logical_writes,
last_logical_writes, min_logical_writes,
max_logical_writes, total_logical_reads,
last_logical_reads, min_logical_reads,
max_logical_reads, total_elapsed_time,
last_elapsed_time, min_elapsed_time,
max_elapsed_time, total_rows,
last_rows, min_rows,
max_rows from sys.dm_exec_query_stats
CROSS APPLY sys.dm_exec_sql_text(sql_handle) qt
CROSS APPLY sys.dm_exec_query_plan(plan_handle) qp
where 
qp.query_plan.exist('//RelOp[@LogicalOp="Index Scan"
		    or @LogicalOp="Clustered Index Scan"
		    or @LogicalOp="Table Scan"]')=1
and 
qp.query_plan.exist('//ColumnReference[fn:lower-case(@Database)=fn:lower-case(sql:variable("@DatabaseName"))]')=1
)
GO



/*** Use this to retrieve query plans from the cache ***/

select query_plan,[text],total_worker_time 
from dbo.ScanInCacheFromDatabase('[ProfitWise]')
order by [total_worker_time] desc


/*** Query spits out the number of pages referred to by all of a table's indexes ***/

DECLARE @databasename varchar(100) = 'ProfitWise', 
		@tablename varchar(100) = 'dbo.profitwiseproduct';

SELECT index_level,index_type_desc,alloc_unit_type_desc,page_count 
FROM sys.dm_db_index_physical_stats
		(DB_ID(@databasename), OBJECT_ID(@tablename), NULL , NULL , NULL);
	 

/*** Lock Analyzer ***/
select resource_type,db_name(resource_database_id) [database],request_mode,
request_session_id, request_status, resource_associated_entity_id, indexes.name index_name
	from sys.dm_tran_locks
	left join sys.partitions on partitions.hobt_id = dm_tran_locks.resource_associated_entity_id
join sys.indexes on indexes.object_id = partitions.object_id and indexes.index_id = partitions.index_id
	where db_name(resource_database_id)='ProfitWise'
