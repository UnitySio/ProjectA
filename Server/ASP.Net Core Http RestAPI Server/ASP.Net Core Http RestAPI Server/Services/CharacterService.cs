using ASP.Net_Core_Http_RestAPI_Server.DBContexts;
using ASP.Net_Core_Http_RestAPI_Server.DBContexts.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ASP.Net_Core_Http_RestAPI_Server.Services;

public class CharacterService
{
    private ILogger<CharacterService> log;
    
    private AccountInfoMapper accountInfoMapper;
    private UserCharacterInfoMapper userCharacterInfoMapper;
    private UserInfoMapper userInfoMapper;
    private UserSigninLogMapper userSigninLogMapper;

    //Construct Dependency Injections
    public CharacterService(ILogger<CharacterService> log, AccountInfoMapper accountInfoMapper, UserCharacterInfoMapper userCharacterInfoMapper, UserInfoMapper userInfoMapper, UserSigninLogMapper userSigninLogMapper)
    {
        this.log = log;
        this.accountInfoMapper = accountInfoMapper;
        this.userCharacterInfoMapper = userCharacterInfoMapper;
        this.userInfoMapper = userInfoMapper;
        this.userSigninLogMapper = userSigninLogMapper;
    }
    
    
    
    
    
    //logic methods
    
    
    
    
    
    
}