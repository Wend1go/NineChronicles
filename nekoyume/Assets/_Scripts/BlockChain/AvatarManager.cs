using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Libplanet;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Crypto;
using Libplanet.Tx;
using Nekoyume.Action;
using Nekoyume.Model;
using Nekoyume.State;
using UniRx;
using UnityEngine;

namespace Nekoyume
{
    /// <summary>
    /// Avatar들을 관리한다.
    /// ToDo. 추후 복수의 아바타를 오갈 수 있도록 한다.
    /// </summary>
    public class AvatarManager
    {
        public const string PrivateKeyFormat = "private_key_{0}";
        public const string AvatarFileFormat = "avatar_{0}.dat";
        
        public static event Action<AvatarState> DidAvatarStateLoaded = delegate { };
        
        private static int _currentAvatarIndex = -1;
        private static PrivateKey _avatarPrivateKey;
        private static string _saveFilePath;
        
        private static IDisposable _disposableForEveryRender;
        
        public static bool InitAvatarPrivateKeyAndFilePath(int index)
        {
            if (_currentAvatarIndex == index)
            {
                return false;
            }
            
            _currentAvatarIndex = index;

            var key = string.Format(PrivateKeyFormat, index);
            var privateKeyHex = PlayerPrefs.GetString(key, "");

            if (string.IsNullOrEmpty(privateKeyHex))
            {
                _avatarPrivateKey = new PrivateKey();
                PlayerPrefs.SetString(key, ByteUtil.Hex(_avatarPrivateKey.ByteArray));
            }
            else
            {
                _avatarPrivateKey = new PrivateKey(ByteUtil.ParseHex(privateKeyHex));
            }

            States.CurrentAvatar.Value.address = _avatarPrivateKey.PublicKey.ToAddress();   
            
            var fileName = string.Format(AvatarFileFormat, index);
            _saveFilePath = Path.Combine(Application.persistentDataPath, fileName);

            return true;
        }
        
        public static void InitAvatarState(int index)
        {
            if (!InitAvatarPrivateKeyAndFilePath(index))
            {
                return;
            }
            
            var avatarState = LoadAvatar(_saveFilePath);
            if (ReferenceEquals(avatarState, null))
            {
                throw new NullReferenceException("LoadAvatar() returns null.");
            }
            
            States.CurrentAvatar.Value = avatarState;
        }

        public static Transaction<PolymorphicAction<ActionBase>> MakeTransaction(
            IEnumerable<PolymorphicAction<ActionBase>> actions,
            BlockChain<PolymorphicAction<ActionBase>> chain
        )
        {
            return Transaction<PolymorphicAction<ActionBase>>.Create(
                chain.GetNonce(_avatarPrivateKey.PublicKey.ToAddress()),
                _avatarPrivateKey,
                actions,
                timestamp: DateTime.UtcNow
            );
        }
        
        /// <summary>
        /// FixMe. 모든 액션에 대한 랜더 단계에서 아바타 주소의 상태를 얻어 오고 있음.
        /// 모든 액션 생성 단계에서 각각의 변경점을 업데이트 하는 방향으로 수정해볼 필요성 있음.
        /// CreateNovice와 HackAndSlash 액션의 처리를 개선해서 테스트해 볼 예정.
        /// 시작 전에 양님에게 문의!
        /// </summary>
        public static void SubscribeAvatarUpdates()
        {
            if (States.CurrentAvatar.Value != null)
            {
                DidAvatarStateLoaded(States.CurrentAvatar.Value);
            }

            if (!ReferenceEquals(_disposableForEveryRender, null))
            {
                return;
            }
            _disposableForEveryRender = ActionBase.EveryRender(States.CurrentAvatar.Value.address).ObserveOnMainThread().Subscribe(eval =>
            {
                var avatarState = (AvatarState) eval.OutputStates.GetState(States.CurrentAvatar.Value.address);
                if (avatarState is null)
                {
                    return;
                }
                PostActionRender(avatarState);
            });
        }
        
        private static void PostActionRender(AvatarState avatarState)
        {
            var avatarLoaded = States.CurrentAvatar.Value == null;
            States.CurrentAvatar.Value = avatarState;
            if (avatarLoaded)
            {
                DidAvatarStateLoaded(States.CurrentAvatar.Value);
            }
        }
        
        private static AvatarState LoadAvatar(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            
            var formatter = new BinaryFormatter();
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                var data = (AvatarState) formatter.Deserialize(stream);
                return data;
            }
        }

        /// <summary>
        /// ToDo. `AgentController.Agent.AgentPrivateKey`에 기반해서 계층적 결정성 비밀키를 만들어서 리턴하도록. 
        /// </summary>
        /// <returns></returns>
        private static PrivateKey CreateHierarchicalDeterministicPrivateKey(int param)
        {
            var key = AgentController.Agent.PrivateKey;
            
            return new PrivateKey();
        }
    }
}
