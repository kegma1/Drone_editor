using UnityEngine;
using Google.OrTools.Sat;

public class ORToolsTest : MonoBehaviour
{
    void Start()
    {
        // Step 1: Create a model
        CpModel model = new CpModel();

        // Step 2: Define variables
        IntVar x = model.NewIntVar(0, 10, "x");
        IntVar y = model.NewIntVar(0, 10, "y");

        // Step 3: Add constraints
        model.Add(x + y == 10);
        model.Minimize(x);

        // Step 4: Solve the model
        CpSolver solver = new CpSolver();
        CpSolverStatus status = solver.Solve(model);

        // Step 5: Output results
        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
            Debug.Log("✔ OR-Tools Test Passed:");
            Debug.Log("x = " + solver.Value(x));
            Debug.Log("y = " + solver.Value(y));
        }
        else
        {
            Debug.Log("✘ No solution found.");
        }
    }
}
