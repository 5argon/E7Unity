using E7.PlayerPrefsExtension;
using NUnit.Framework;
using UnityEngine;

public class StateHolder
{
    public enum State
    {
        StateA,
        StateB,
        StateC
    }

    public enum Index
    {
        IndexA,
        IndexB,
        IndexC,
    }
}

public class PlayerPrefsExtensionTests
{
    [SetUp]
    public void CleanPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private enum State
    {
        StateA,
        StateB,
        StateC
    }

    private enum State2
    {
        StateA,
        StateB,
        StateC
    }

    private enum SubState
    {
        Sub1,
        Sub2,
        Sub3,
    }

    private enum SubState2
    {
        Sub1,
        Sub2,
        Sub3,
    }

    private enum SubStateMissing
    {
        Sub1,
        Sub2,
    }

    private enum Index
    {
        IndexA,
        IndexB,
        IndexC,
    }

    private enum Index2
    {
        IndexA,
        IndexB,
        IndexC,
    }

    [Test]
    public void GetSetState()
    {
        SetPersistent.State(State.StateA, true);
        Assert.That(Persistent.State(State.StateA), Is.True);
    }

    [Test]
    public void PrefsClearing()
    {
        SetPersistent.State(State.StateA, true);
        SetPersistent.State(State2.StateB, true);
        SetPersistent.State(StateHolder.State.StateA, true);
        PlayerPrefs.DeleteAll();
        Assert.That(Persistent.State(State.StateA) , Is.False);
        Assert.That(Persistent.State(State2.StateB) , Is.False);
        Assert.That(Persistent.State(StateHolder.State.StateA) , Is.False);
    }

    [Test]
    public void PrefsDeleting()
    {
        SetPersistent.State(State.StateA, true);
        SetPersistent.State(State2.StateB, true);
        SetPersistent.State(StateHolder.State.StateA, true);
        Persistent.Delete(State2.StateB);
        Assert.That(Persistent.State(State.StateA) , Is.True);
        Assert.That(Persistent.State(State2.StateB) , Is.False);
        Assert.That(Persistent.State(StateHolder.State.StateA) , Is.True);
    }

    [Test]
    public void DifferentEnumInSameClass()
    {
        SetPersistent.Index(Index.IndexA, 555);
        SetPersistent.Index(Index2.IndexA, 666);
        Assert.That(Persistent.Index(Index.IndexA), Is.EqualTo(555));
        Assert.That(Persistent.Index(Index2.IndexA), Is.EqualTo(666));
    }

    [Test]
    public void DifferentEnumInDifferentClass()
    {
        SetPersistent.Index(Index.IndexA, 555);
        SetPersistent.Index(StateHolder.Index.IndexA, 666);
        Assert.That(Persistent.Index(Index.IndexA), Is.EqualTo(555));
        Assert.That(Persistent.Index(StateHolder.Index.IndexA), Is.EqualTo(666), "Proof that using assembly qualified name works");
    }

    [Test]
    public void WrongType()
    {
        SetPersistent.Index(Index.IndexA, 555);
        Assert.That(Persistent.State(Index.IndexA), Is.False);

        SetPersistent.Index(Index.IndexA, 1);
        Assert.That(Persistent.State(Index.IndexA), Is.True, "Number 1 is unfortunately usable with true state.");

        SetPersistent.State(Index.IndexA, true);
        Assert.That(Persistent.Index(Index.IndexA), Is.EqualTo(1), "True state is unfortunately usable with 1 index.");

        SetPersistent.State(Index.IndexA, false);
        Assert.That(Persistent.Index(Index.IndexA), Is.Zero);
    }

    [Test]
    public void EnumStateDefault()
    {
        Assert.That(Persistent.State<State, SubState>(State.StateB), Is.EqualTo(default(SubState)));
    }

    [Test]
    public void EnumState()
    {
        SetPersistent.State(State.StateB, SubState.Sub3);
        Assert.That(Persistent.State<State, SubState>(State.StateB), Is.EqualTo(SubState.Sub3));
    }

    [Test]
    public void EnumStateWrongType()
    {
        SetPersistent.State(State.StateB, SubState.Sub3);
        Assert.That(Persistent.State<State, SubState2>(State.StateB), Is.EqualTo(SubState2.Sub3),
        "Because it remembers by just the enum's name, same name results in a parsable enum.");
    }

    [Test]
    public void EnumStateMissing()
    {
        SetPersistent.State(State.StateB, SubState.Sub3);
        Assert.That(Persistent.State<State, SubStateMissing>(State.StateB), Is.EqualTo(default(SubStateMissing)));
    }

}
