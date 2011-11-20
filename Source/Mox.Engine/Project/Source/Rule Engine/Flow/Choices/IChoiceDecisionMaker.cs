namespace Mox.Flow
{
    public interface IChoiceDecisionMaker
    {
        object MakeChoiceDecision(Sequencer sequencer, Choice choice);
    }
}