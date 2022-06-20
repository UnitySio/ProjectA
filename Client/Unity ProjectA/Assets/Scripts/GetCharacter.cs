using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using ASP.Net_Core_Http_RestAPI_Server.JsonDataModels;
using UnityEngine;

public class GetCharacter : MonoBehaviour
{
    public List<CharacterData> characterData;

    public RectTransform content;
    public CharacterListItem item;
    
    private void Start()
    {
        GetCharacters();
    }

    public async Task GetCharacters()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);

        if (JWTManager.CheckValidateJWT(accessToken))
        {
            var request = new RequestGetCharacter()
            {
                jwtAccess = jwtAccess
            };
            
            var response =
                await APIManager.SendAPIRequestAsync(API.GetCharacter, request, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as ResponseGetCharacter;

                var text = result.result;

                if (text.Equals("ok"))
                    if (result.characterData.Count > 0)
                    {
                        characterData = result.characterData;
                        CharacterList();
                    }
            }
        }
    }

    public void CharacterList()
    {
        for (int i = 0; i < characterData.Count; i++)
        {
            var inst = Instantiate(item, content);
            inst.uid = (int)characterData[i].CharacterUniqueID;
            inst.grade = characterData[i].CharacterGrade;
            inst.lv = characterData[i].CharacterLv;
        }
    }

    public async Task AddCharacter()
    {
        var jwtAccess = SecurityPlayerPrefs.GetString("JWTAccess", null);
        JwtSecurityToken accessToken = JWTManager.DecryptJWT(jwtAccess);

        if (JWTManager.CheckValidateJWT(accessToken))
        {
            var request = new RequestAddCharacter()
            {
                jwtAccess = jwtAccess,
                characterUniqueID = 1,
                characterLv = 30
            };

            var response =
                await APIManager.SendAPIRequestAsync(API.AddCharacter, request, ServerManager.Instance.FailureCallback);

            if (response != null)
            {
                var result = response as ResponseAddCharacter;

                var text = result.result;
                
                if (text.Equals("ok"))
                {
                    Debug.Log("정상적으로 캐릭터를 데이터베이스에 추가했습니다.");
                }
                else
                    Debug.Log("해당 캐릭터가 이미 존재합니다");
            }
        }
    }
}
