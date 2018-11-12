using System.Collections;
using System.Collections.Generic;
using Nekoyume.Model;
using Nekoyume.Network.Agent;
using Nekoyume.Move;
using Planetarium.Crypto.Extension;
using Planetarium.Crypto.Keys;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace Nekoyume.Game
{
    public class Stage : MonoBehaviour
    {
        public FollowCamera followCam = null;
        public GameObject avatar;
        public GameObject background = null;

        private Agent agent;

        private string zone;
        private User user;

        public void Awake()
        {
            var serverUrl = "http://localhost:4000";
            var privateKeyHex = PlayerPrefs.GetString("private_key", "");

            PrivateKey privateKey = null;
            if (string.IsNullOrEmpty(privateKeyHex))
            {
                privateKey = PrivateKey.Generate();
            }
            else
            {
                privateKey = PrivateKey.FromBytes(privateKeyHex.ParseHex());
            }
            agent = new Agent(serverUrl, privateKey);
        }
        public void Start()
        {
            InitCamera();
            var key = PrivateKey.Generate();
            user = new User(key);
            LoadBackground("nest");
            StartCoroutine(agent.Run());
        }

        public void InitCamera()
        {
            followCam = Camera.main.gameObject.GetComponent<FollowCamera>();
            if (followCam == null)
            {
                followCam = Camera.main.gameObject.AddComponent<FollowCamera>();
            }
        }

        public void LoadBackground(string prefabName)
        {
            if (background != null)
            {
                if (background.name.Equals(prefabName))
                {
                    return;
                }
                Destroy(background);
                background = null;
            }
            string resName = string.Format("Prefab/Background/{0}", prefabName);
            GameObject prefab = Resources.Load<GameObject>(resName);
            if (prefab != null)
            {
                background = Instantiate(prefab, transform);
                background.name = prefabName;
            }
            var camPosition = followCam.transform.position;
            camPosition.x = 0;
            followCam.transform.position = camPosition;
        }

        public void OnLogin(Network.Response.Login response)
        {
            LoadBackground("room");
            zone = response.avatar.zone;
            var character = avatar.GetComponent<Character>();
            StartCoroutine(character.Load(avatar, response.avatar.class_));
            UI.Widget.Create<UI.Move>().Show();
        }

        public void OnLogin()
        {
            LoadBackground("room");
            var move = user.CreateNovice(new Dictionary<string, string>
            {
                {"name", "test"}
            });
            var _avatar = user.Avatar(new List<Move.Move> { move });
            var jobChange = user.FirstClass(CharacterClass.Swordman.ToString());
            _avatar = user.Avatar(new List<Move.Move> { move, jobChange });
            var character = avatar.GetComponent<Character>();
            StartCoroutine(character.Load(avatar, _avatar.class_));
            UI.Widget.Create<UI.Move>().Show();
        }

        public void Move()
        {
            var character = avatar.GetComponent<Character>();
            StartCoroutine(character.Walk());
            followCam.target = avatar.transform;
            LoadBackground(zone);
        }

        public void OnHackAndSlash(Network.Response.LastStatus response)
        {
            var character = avatar.GetComponent<Character>();
            StartCoroutine(character.Stop());
            StartCoroutine(ProcessStatus(response));
        }

        private IEnumerator ProcessStatus(Network.Response.LastStatus lastStatus)
        {
            foreach (var battleStatus in lastStatus.status)
            {
                var statusType = battleStatus.GetStatusType();
                if (statusType != null)
                {
                    var status = (Status.Base)System.Activator.CreateInstance(statusType);
                    if (status != null)
                    {
                        yield return status.Execute(this, battleStatus);
                    }
                }
            }
        }

        public void Home()
        {

        }

        public void SendMove(Move.Move move)
        {
            agent.Send(move);
        }
    }
}
