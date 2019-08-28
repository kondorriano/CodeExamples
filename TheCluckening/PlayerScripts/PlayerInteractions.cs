using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] private float _talkLookAngle = 40;

    //Variables
    bool startTalking = false;

    //Interaction List
    List<IInteraction> _nearInteractions;
    IInteraction _nearestInteraction = null;

    //Components
    private PlayerController _playerController;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;

        //Prepare Interaction List
        _nearInteractions = new List<IInteraction>();
    }

    public void EarlyTick(bool interactDown)
    {
        //InEndLocked
        if (_playerController.IsPlayerEndLocked == true)
        {
            return;
        }

        //PlayerDisabled
        if (_playerController.IsPlayerDisabled == true)
        {
            return;
        }

        /*PLAYER TICK*/
        if (_playerController.IsPlayerBig == true)
        {
            EarlyTickBeast(interactDown);
        }
        else
        {
            EarlyTickChicken(interactDown);
        }
    }

    public void LateTick(bool acceptDown)
    {
        /*PLAYER TICK*/
        if (_playerController.IsPlayerBig == true)
        {
            return;
        }

        if(startTalking == true)
        {
            startTalking = false;
            return;
        }

        LateTickChicken(acceptDown);
    }

    public void EarlyTickBeast(bool interactDown)
    {
        //Talking
        if (_playerController.InDialogue == true)
        {
            return;
        }

        //Set Nearest 
        SetNearestNPChicken(GetNearestInteractions(false));
        //Not talking
        //No NPCs
        if (_nearestInteraction == null || _nearestInteraction.Equals(null))
        {
            return;
        }
        //In an action
        if (_playerController.IsDoingAction == true)
        {
            return;
        }

        //Not in ground
        if (_playerController.TouchingGround == false)
        {
            return;
        }

        if (interactDown == true)
        {
            Variables.InteractionTypes type = _nearestInteraction.GetInteractionType();

            if (type == Variables.InteractionTypes.Action) _nearestInteraction.TriggerAction(_playerController);
        }
    }


    public void EarlyTickChicken(bool interactDown)
    {
        //Talking
        if (_playerController.InDialogue == true)
        {
            return;
        }

        //Set Nearest 
        SetNearestNPChicken(GetNearestInteractions(true));

        //Not talking
        //No NPCs
        if (_nearestInteraction == null || _nearestInteraction.Equals(null))
        {
            return;
        }
        //In an action
        if (_playerController.IsDoingAction == true)
        {
            return;
        }

        //Not in ground
        if (_playerController.TouchingGround == false)
        {
            return;
        }

        if(interactDown == true)
        {
            Variables.InteractionTypes type = _nearestInteraction.GetInteractionType();

            if (type == Variables.InteractionTypes.Action) _nearestInteraction.TriggerAction(_playerController);
            else if (type == Variables.InteractionTypes.Dialogue && !_playerController.InCredits)
            {
                _nearestInteraction.PreDialogue(_playerController);
                string npcName = _nearestInteraction.GetNPCName();
                string[] dialogue = _nearestInteraction.GetCurrentDialogue();
                //Variables.QuestVoices[] voices = _nearestInteraction.GetCurrentAudio();
                startTalking = true;
                //Start Interaction
                _playerController.StartDialogue(npcName, dialogue);
            }
            
        }
    }

    public bool StartInteraction(IInteraction interaction)
    {
        if (_playerController.IsPlayerBig) return false;
        if (_playerController.InDialogue) return false;
        if (_playerController.IsDoingAction) return false;

        _nearestInteraction = interaction;
        _nearestInteraction.PreDialogue(_playerController);
        string npcName = _nearestInteraction.GetNPCName();
        string[] dialogue = _nearestInteraction.GetCurrentDialogue();
        //Variables.QuestVoices[] voices = _nearestInteraction.GetCurrentAudio();
        startTalking = true;
        //Start Interaction
        _playerController.StartDialogue(npcName, dialogue);
        return true;
    }

    public void LateTickChicken(bool acceptDown)
    {
        //Talking
        if (_playerController.InDialogue == true)
        {
            if (acceptDown == true)
            {
                //HitNext
                _playerController.NextLine();
            }
        }
    }

    public void FinishingDialogue()
    {
        if (_nearestInteraction != null) _nearestInteraction.PostDialogue();
        _playerController.SetExtraTransition(null, Variables.CameraTransition.Fast);
    }

    public string GetInteractionSound(int id)
    {
        if (_nearestInteraction == null) return "";
        return _nearestInteraction.GetCurrentAudio(id);
    }

    public void SetCameraTransition(int id)
    {
        //SET CURVE IF NULL?
        if (_nearestInteraction == null)
        {
            _playerController.SetExtraTransition(null, Variables.CameraTransition.Fast);
            return;
        }
        Variables.ExtraCameraTransition transitionData = _nearestInteraction.GetTransitionData(id);
        Variables.CameraTransition transitionType = (transitionData == null) ? Variables.CameraTransition.Fast : transitionData.transitionType;
        _playerController.SetExtraTransition(transitionData, transitionType);
    }

    #region List Stuff

    public void AddInteraction(IInteraction interaction)
    {
        if (_playerController.IsPlayerEndLocked == true) return;
        if (_playerController.IsPlayerDisabled == true) return;
        if (_playerController.InDialogue == true) return;
        if (!_nearInteractions.Contains(interaction)) _nearInteractions.Add(interaction);
    }

    public void RemoveInteraction(IInteraction interaction)
    {
        if (_playerController.IsPlayerEndLocked == true) return;
        if (_playerController.IsPlayerDisabled == true) return;
        if (_playerController.InDialogue == true) return;
        if (_nearInteractions.Contains(interaction)) _nearInteractions.Remove(interaction);
    }

    public void ClearNPChickens()
    {
        _nearInteractions.Clear();
    }

    IInteraction GetNearestInteractions(bool includeDialogue)
    {
        if (_nearInteractions.Count <= 0) return null;
        int nearestChickenIndex = -1;
        Transform playerTransform = transform;

        for (int i = 0; i < _nearInteractions.Count; ++i)
        {
            if (_nearInteractions[i] == null || _nearInteractions[i].Equals(null))
            {
                continue;
            }

            if(includeDialogue == false && _nearInteractions[i].GetInteractionType() == Variables.InteractionTypes.Dialogue)
            {
                continue;
            }

            Vector3 npcDdirection = (_nearInteractions[i].GetTransform().position - playerTransform.position);
            if(npcDdirection.magnitude > 4)
            {
                _nearInteractions[i] = null;
                continue;
            }
            npcDdirection.y = 0;
            npcDdirection.Normalize();

            Vector3 lookDirection = transform.forward;
            lookDirection.y = 0;
            lookDirection.Normalize();

            if(Vector3.Angle(lookDirection, npcDdirection) > _talkLookAngle)
            {
                continue;
            }

            if (nearestChickenIndex == -1)
            {
                nearestChickenIndex = i;
                continue;
            }

            float dist1 = (_nearInteractions[nearestChickenIndex].GetTransform().position - playerTransform.position).sqrMagnitude;
            float dist2 = (_nearInteractions[i].GetTransform().position - playerTransform.position).sqrMagnitude;

            if (dist1 >= dist2) nearestChickenIndex = i;
        }

        if (nearestChickenIndex != -1) return _nearInteractions[nearestChickenIndex];
        return null;
    }

    void SetNearestNPChicken(IInteraction newNearestChicken)
    {
        if (_nearestInteraction == newNearestChicken) return;

        _nearestInteraction = newNearestChicken;        
    }

    public Transform GetNearestInteractable(out string text)
    {
        if (_nearestInteraction == null || _nearestInteraction.Equals(null))
        {
            text = "";
            return null;
        }

        text = _nearestInteraction.GetInteractionText();
        return _nearestInteraction.GetTransform();
    }
    #endregion

    public void SwapCharacters(bool isPlayerBig)
    {
        ClearNPChickens();        
        SetNearestNPChicken(null);
    }

    public void PlayerRespawn()
    {
        ClearNPChickens();
        SetNearestNPChicken(null);
    }

}