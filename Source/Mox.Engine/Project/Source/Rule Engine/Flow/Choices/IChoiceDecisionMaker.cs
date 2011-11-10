namespace Mox.Flow
{
    public interface IChoiceDecisionMaker
    {
        object MakeChoiceDecision(NewSequencer sequencer, Choice choice);
    }
}