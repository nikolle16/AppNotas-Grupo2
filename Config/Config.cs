using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_Notas___Grupo_2.Config
{
    public class Config
    {
        public static string EndPointCreate = "http://192.168.1.41:6000/api/user";
        public static string EndPointList = "http://192.168.1.41:6000/api/user";
        public static string EndPointUpdate = "http://192.168.1.41:6000/api/user";
        public static string EndPointDelete = "http://192.168.1.41:6000/api/user";
        public static string EndPointCreateNote = "http://192.168.1.41:6000/api/note";
        public static string EndPointListNotes = "http://192.168.1.41:6000/api/note";
        public static string EndPointGetNotes = "http://192.168.1.41:6000/api/note";
        public static string EndPointGetNote = "http://192.168.1.41:6000/api/note";
        public static string EndPointUpdateNote = "http://192.168.1.41:6000/api/note";
        public static string EndPointDeleteNote = "http://192.168.1.41:6000/api/note";
        public static string EndPointCreateAudio = "http://192.168.1.41:6000/api/audios";
        public static string EndPointGetAudios = "http://192.168.1.41:6000/api/audios";
        public static string EndPointDeleteAudio = "http://192.168.1.41:6000/api/audios";

        public static string BearerToken = "brrr";
    }
}
