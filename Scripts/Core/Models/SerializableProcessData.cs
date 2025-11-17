using System.Collections.Generic;
using UnityEngine;

namespace VRBuilder.MCP.Core.Models
{
	/// <summary>
	/// Serializable DTOs for process data suitable for JSON transport.
	/// Unity types like Vector2 are flattened to simple {x,y} objects.
	/// </summary>
	public class SerializableVector2
	{
		public float x { get; set; }
		public float y { get; set; }

		public static SerializableVector2 FromVector2(Vector2 v)
		{
			return new SerializableVector2 { x = v.x, y = v.y };
		}
	}

	public class SerializableStepData
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public SerializableVector2 Position { get; set; }
	}

	public class SerializableChapterData
	{
		public string Name { get; set; }
		public List<SerializableStepData> Steps { get; set; } = new List<SerializableStepData>();
	}

	public class SerializableProcessData
	{
		public string ProcessName { get; set; }
		public List<SerializableChapterData> Chapters { get; set; } = new List<SerializableChapterData>();

		public static SerializableProcessData FromProcessCreationRequest(ProcessCreationRequest source)
		{
			var dto = new SerializableProcessData
			{
				ProcessName = source?.ProcessName
			};

			if (source?.Chapters != null)
			{
				foreach (var ch in source.Chapters)
				{
					var chapterDto = new SerializableChapterData
					{
						Name = ch?.Name
					};

					if (ch?.Steps != null)
					{
						foreach (var st in ch.Steps)
						{
							var stepDto = new SerializableStepData
							{
								Name = st?.Name,
								Description = st?.Description,
								Position = st?.Position.HasValue == true ? SerializableVector2.FromVector2(st.Position.Value) : null
							};
							chapterDto.Steps.Add(stepDto);
						}
					}

					dto.Chapters.Add(chapterDto);
				}
			}

			return dto;
		}
	}
}