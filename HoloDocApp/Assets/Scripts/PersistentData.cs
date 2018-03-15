using System;
using UnityEngine;

public static class PersistentData {
	public static string DefaultAuthor = "Default author";//Environment.MachineName;

	public static string ServerIp = "localhost";
	public static string ServerPort = "8080";

    public static Color WorkspaceBackgroundColor { get; set; }
}
