namespace Duck.Ecs;

public class FilterEvaluator
{
    public bool Evaluate(IFilter filter, IEntity entity)
    {
        foreach (var componentPredicate in filter.ComponentPredicates) {
            if (!componentPredicate(entity)) {
                return false;
            }
        }

        return true;
    }
}
