//using System.Collections.Generic;
//using UnityEngine;

//public class EvidenceReceiver : MonoBehaviour
//{
//    public string npcId; // "Receptionist", "Police", etc.

//    [System.Serializable]
//    public class EvidenceReaction
//    {
//        public EvidenceItem evidence;
//        public DialogueNode onPresent;
//        public DialogueNode onRepeat;
//    }

//    public List<EvidenceReaction> reactions = new();

//    private HashSet<EvidenceItem> alreadySeen = new();

//    public bool TryPresent(EvidenceItem item, DialogueManager dialogue)
//    {
//        var reaction = reactions.Find(r => r.evidence == item);
//        if (reaction == null) return false;

//        if (alreadySeen.Contains(item))
//        {
//            if (reaction.onRepeat != null)
//                dialogue.StartDialogue(reaction.onRepeat);
//        }
//        else
//        {
//            alreadySeen.Add(item);
//            if (reaction.onPresent != null)
//                dialogue.StartDialogue(reaction.onPresent);
//        }

//        return true;
//    }
//}
