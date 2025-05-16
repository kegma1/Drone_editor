// using System.Collections.Generic;
// using UnityEngine;

// public class CollisionPrediction
// {
//     public static bool CheckCollisions(List<DronePath> dronePaths, float collisionThreshold = 1f)
//     {
//         // Sample resolution, more samples = more accurate collision detection
//         float sampleResolution = 0.1f; // Sample every 0.1 seconds along the path

//         // Generate path samples for each drone
//         List<List<Vector3>> droneSamples = new List<List<Vector3>>();
//         foreach (var path in dronePaths)
//         {
//             List<Vector3> samples = GeneratePathSamples(path, sampleResolution);
//             droneSamples.Add(samples);
//         }

//         // Check for collisions between all drones' sampled positions
//         for (int i = 0; i < droneSamples.Count; i++)
//         {
//             for (int j = i + 1; j < droneSamples.Count; j++)
//             {
//                 if (DetectCollisionBetweenPaths(droneSamples[i], droneSamples[j], collisionThreshold))
//                 {
//                     Debug.Log($"Collision detected between Drone {i} and Drone {j}");
//                     return true; // If any collision detected, return true
//                 }
//             }
//         }

//         return false; // No collision found
//     }

//     private static List<Vector3> GeneratePathSamples(DronePath path, float sampleResolution)
//     {
//         List<Vector3> samples = new List<Vector3>();

//         for (float t = 0f; t <= 1f; t += sampleResolution)
//         {
//             Vector3 samplePoint = PathGenome.BezierEvaluate(path.Start, path.ControlPoints[0], path.ControlPoints[1], t);
//             samples.Add(samplePoint);
//         }

//         return samples;
//     }

//     private static bool DetectCollisionBetweenPaths(List<Vector3> pathA, List<Vector3> pathB, float threshold)
//     {
//         for (int i = 0; i < pathA.Count; i++)
//         {
//             for (int j = 0; j < pathB.Count; j++)
//             {
//                 if (Vector3.Distance(pathA[i], pathB[j]) < threshold)
//                 {
//                     return true; // Collision detected
//                 }
//             }
//         }

//         return false; // No collision detected
//     }
// }
