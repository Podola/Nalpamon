using Yarn.Unity;

public interface IPresenterEvent
{
    public void LineEvent(LocalizedLine line);

    public YarnTask LineWait(LineCancellationToken token);
}
