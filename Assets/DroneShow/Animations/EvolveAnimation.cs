using System.Collections.Generic;
using UnityEngine;

public enum PathStyle
{
    SmoothArc,
    SwaggySwoop,
    ZigZag,
}



public class EvolveAnimation : MonoBehaviour, IAnimation
{
    public float Speed { get; set; } = 3f;
    public Dictionary<Vector3, DronePath> Paths { get; set; } = new();

    private int populationSize = 20;
    private int generations = 200;

    [SerializeField]
    private PathStyle style = PathStyle.SwaggySwoop;
    public PathStyle Style { get => style; set => style = value; }



    public DronePath GeneratePath(Vector3 from, Vector3 to)
    {
        List<PathGenome> population = GenerateInitialPopulation(from, to);

        for (int g = 0; g < generations; g++)
        {
            foreach (var individual in population)
            {
                individual.Fitness = EvaluateFitness(individual, from, to);
            }

            population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

            var survivors = population.GetRange(0, populationSize / 2);

            List<PathGenome> nextGen = new(survivors);
            while (nextGen.Count < populationSize)
            {
                var parent = survivors[Random.Range(0, survivors.Count)];
                var offspring = Mutate(parent, from, to);
                nextGen.Add(offspring);
            }

            population = nextGen;
        }

        if (population.Count == 0)
        {
            Debug.LogWarning("No valid paths generated.");
            return null;
        }

        var best = population[0];
        return best.ToDronePath(from, to);
    }

    private List<PathGenome> GenerateInitialPopulation(Vector3 start, Vector3 end)
{
    var population = new List<PathGenome>();
    int segments = 2; //

    for (int i = 0; i < populationSize; i++)
    {
        Vector3 dir = end - start;
        Vector3 right = Vector3.Cross(dir.normalized, Vector3.up);

        var genome = new PathGenome();

        for (int s = 0; s < segments; s++)
        {
            float tA = (s + 1f) / (segments + 1f);
            float tB = (s + 1.5f) / (segments + 1f);

            Vector3 posA = Vector3.Lerp(start, end, tA);
            Vector3 posB = Vector3.Lerp(start, end, tB);

            Vector3 lateralOffset = right * Random.Range(-2f, 2f);
            Vector3 verticalOffset = Vector3.up * Random.Range(1f, 5f);

            genome.ControlPoints.Add(posA + lateralOffset + verticalOffset);
            genome.ControlPoints.Add(posB - lateralOffset + verticalOffset);
        }

        population.Add(genome);
    }

    return population;
}


    private PathGenome Mutate(PathGenome original, Vector3 start, Vector3 end)
{
    PathGenome mutant = new PathGenome();

    foreach (var point in original.ControlPoints)
    {
        Vector3 mutated = point + Random.insideUnitSphere * 0.5f;
        mutant.ControlPoints.Add(mutated);
    }

    return mutant;
}


    private float EvaluateFitness(PathGenome genome, Vector3 start, Vector3 end)
{
    Vector3 lastStart = start;
    float fitness = 0f;
    int segmentCount = genome.ControlPoints.Count / 2;
    Vector3 dir = end - start;

    for (int i = 0; i < genome.ControlPoints.Count; i += 2)
    {
        Vector3 controlA = genome.ControlPoints[i];
        Vector3 controlB = genome.ControlPoints[i + 1];

        switch (Style)
        {
            case PathStyle.SmoothArc:
                float smoothness = -Vector3.Distance(controlA, controlB);
                float arcHeight = (controlA.y + controlB.y) * 0.5f;
                fitness += smoothness * 0.7f + arcHeight * 0.3f;
                break;

            case PathStyle.SwaggySwoop:
                float symmetry = -Mathf.Abs((controlA - start).magnitude - (controlB - end).magnitude);
                float loopiness = Vector3.Cross(controlB - controlA, dir).magnitude;
                fitness += symmetry * 0.5f + loopiness * 0.5f;
                break;

            case PathStyle.ZigZag:
                float zig = Mathf.Abs(controlA.x - controlB.x);
                float zag = Mathf.Abs(controlA.z - controlB.z);
                float chaos = (zig + zag);
                float midHeight = (controlA.y + controlB.y) * 0.5f;
                fitness += chaos * 0.6f + midHeight * 0.4f;
                break;

            default:
                break;
        }
    }

    fitness /= segmentCount;

    for (int i = 0; i < genome.ControlPoints.Count; i += 2)
    {
        Vector3 a = genome.ControlPoints[i];
        Vector3 b = genome.ControlPoints[i + 1];
        lastStart = PathGenome.BezierEvaluate(lastStart, a, b, 1f);
    }

    float endDistance = Vector3.Distance(lastStart, end);
    float endReward = Mathf.Clamp01(1f - (endDistance / 10f)); 
    fitness += endReward * 20f; //reward for being close

    //if (endDistance > 0.1f)
    //{
    //    fitness -= endDistance * 1000f; //maybe a little bit mean
    //}
        

    return fitness;
}

}
