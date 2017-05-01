public interface IState {
	void Enter();

	void  Execute();

	void Exit();
}

public class StateMachine {
	public IState currentState;

	public void ChangeState (IState newState) {
		if (currentState != null)
			currentState.Exit ();

		currentState = newState;
		currentState.Enter ();
	}

	public void ExecuteCurrent () {
		if (currentState != null)
			currentState.Execute ();
	}
}