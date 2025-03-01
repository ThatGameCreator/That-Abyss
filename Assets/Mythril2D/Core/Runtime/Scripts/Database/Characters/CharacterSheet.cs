using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    [System.Serializable]
    public struct AbilityEntry
    {
        public AbilitySheet ability;
        public int level;
    }

    public abstract class CharacterSheet : DatabaseEntry, INameable
    {
        [Header("General")]
        [SerializeField] private EAlignment m_alignment = EAlignment.Default;
        [SerializeField] private string m_displayName = string.Empty;
        [SerializeField] private string m_displayNameKey = string.Empty;
        [SerializeField] private SerializableDictionary<AbilitySheet, int> m_abilitiesPerLevel;

        [Header("Audio")]
        [SerializeField] private AudioClipResolver m_hitAudio;
        [SerializeField] private AudioClipResolver m_deathAudio;

        public EAlignment alignment => m_alignment;
        public string displayName => DisplayNameUtils.GetNameOrDefault(this, m_displayName);
        public string displayNameKey => m_displayNameKey;
        public AudioClipResolver hitAudio => m_hitAudio;
        public AudioClipResolver deathAudio => m_deathAudio;

        public SerializableDictionary<AbilitySheet, int> abilitiesPerLevel
        {
            get => m_abilitiesPerLevel;
            set =>  m_abilitiesPerLevel = value;
        }

        public CharacterSheet(EAlignment alignment)
        {
            m_alignment = alignment;
        }

        public IEnumerable<AbilitySheet> GetAvailableAbilitiesAtLevel(int level)
        {
            return m_abilitiesPerLevel.Where((keyValuePair) => keyValuePair.Value <= level).Select((keyValuePair) => keyValuePair.Key);
        }

        public IEnumerable<AbilitySheet> GetAbilitiesUnlockedAtLevel(int level)
        {
            return m_abilitiesPerLevel.Where((keyValuePair) => keyValuePair.Value == level).Select((keyValuePair) => keyValuePair.Key);
        }

    }
}
