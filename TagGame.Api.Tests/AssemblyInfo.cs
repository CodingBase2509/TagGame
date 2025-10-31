using Xunit;

// Disable test parallelization to avoid race conditions in integration tests
// that mutate shared environment variables (e.g., connection strings) and
// concurrently create/drop databases.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

