

public static class Log {
	enum Level {
		Debug,
		Info,
		Warning,
		Error
	}

	public static void Debug( string message, params object[] args  ) {
		Write( Level.Debug, message, args );
	}

	public static void Warning( string message, params object[] args  ) {
		Write( Level.Warning, message, args );
	}

	public static void Error( string message, params object[] args  ) {
		Write( Level.Error, message, args );
	}

	private static void Write( Level level, string message, object[] args ) {
		if( args != null && args.Length > 0 ) {
			message = string.Format( message, args );
		}

		switch( level ) {
			case Level.Debug:
				UnityEngine.Debug.Log("<color=blue>"+message+"</color>");
				break;
			case Level.Info:
				UnityEngine.Debug.Log(message);
				break;
			case Level.Warning:
				UnityEngine.Debug.LogWarning("<color=yellow>"+message+"</color>");
				break;
			case Level.Error:
				UnityEngine.Debug.LogError(message);
				break;
		}
	}
}