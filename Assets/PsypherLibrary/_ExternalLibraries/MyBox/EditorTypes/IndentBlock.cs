#if UNITY_EDITOR
using System;
using UnityEditor;

namespace PsypherLibrary._ExternalLibraries.MyBox.EditorTypes
{
	public class IndentBlock : IDisposable
	{
		public IndentBlock()
		{
			EditorGUI.indentLevel++;
		}

		public void Dispose()
		{
			EditorGUI.indentLevel--;
		}
	}
}
#endif