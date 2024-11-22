using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace SciencetopiaWebApplication.Controllers.SearchEngine
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IDriver _driver;

        public SearchController(IDriver driver)
        {
            _driver = driver;
        }

        [HttpGet("SearchKnowledgeBase")]
        public async Task<IActionResult> SearchKnowledgeBaseAsync(string query, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required.");

            using (var session = _driver.AsyncSession())
            {
                try
                {
                    var result = await session.RunAsync(
                        @"CALL db.index.fulltext.queryNodes('generalSearchIndex', $query) YIELD node, score
                          WHERE NOT node:pending_approval AND NOT node:disapproved
                          RETURN node, score
                          ORDER BY score DESC
                          SKIP $skip
                          LIMIT $limit",
                        new
                        {
                            query,
                            skip = (page - 1) * pageSize,
                            limit = pageSize
                        }
                    );

                    var searchResults = await result.ToListAsync(record => new
                    {
                        Properties = record["node"].As<INode>().Properties,
                        Labels = record["node"].As<INode>().Labels,
                        Score = record["score"].As<double>()
                    });

                    return Ok(searchResults);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error executing SearchKnowledgeBaseAsync: {ex.Message}");
                    return StatusCode(500, "An error occurred while processing the search request.");
                }
            }
        }

        [HttpGet("SearchResources")]
        public async Task<IActionResult> SearchResourcesAsync(string query, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required.");

            using (var session = _driver.AsyncSession())
            {
                try
                {
                    var result = await session.RunAsync(
                        @"CALL db.index.fulltext.queryNodes('resourceSearchIndex', $query) YIELD node AS directResourceNode, score AS resourceScore
                  CALL db.index.fulltext.queryNodes('generalSearchIndex', $query) YIELD node AS matchedLinkedNode, score AS linkedScore
                  OPTIONAL MATCH (matchedLinkedNode)-[:HAS_RESOURCE]->(linkedResourceNode:Resource)
                  WHERE NOT matchedLinkedNode:pending_approval AND NOT matchedLinkedNode:disapproved
                  WITH collect(directResourceNode) + collect(linkedResourceNode) AS allResources, 
                       resourceScore + COALESCE(linkedScore, 0) AS totalScore
                  UNWIND allResources AS resourceNode
                  RETURN DISTINCT resourceNode, totalScore
                  ORDER BY totalScore DESC
                  SKIP $skip LIMIT $limit",
                        new
                        {
                            query,
                            skip = (page - 1) * pageSize,
                            limit = pageSize
                        }
                    );

                    var searchResults = await result.ToListAsync(record => new
                    {
                        Properties = record["resourceNode"].As<INode>().Properties,
                        Labels = record["resourceNode"].As<INode>().Labels,
                        Score = record["totalScore"].As<double>()
                    });

                    return Ok(searchResults);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error executing SearchResourcesAsync: {ex.Message}");
                    return StatusCode(500, "An error occurred while processing the search request.");
                }
            }
        }
    }
}