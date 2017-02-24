using UnityEngine;

/// <summary>
/// This is often better than just using a static class because it makes it
/// easier to switch from singleton to an instance based approach when the
/// singleton is itself just an instance of a non-static class.
/// 
/// Usage:
/// public class Thing : Singleton<Thing> { }
/// </summary>
public abstract class Singleton<T> where T : new()
{
	private static T _instance;
	public static T instance
	{
		get
		{
			if (_instance == null) _instance = new T();

			return _instance;
		}
	}
}
