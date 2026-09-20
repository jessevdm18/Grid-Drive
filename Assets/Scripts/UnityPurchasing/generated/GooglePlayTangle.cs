// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("iGucSNRAJz56ffSEN38PypDWUfViFUVF9nJ9MYOwB9fnTleKdPBq+x6wBvq6mYMyCQnsyRHKEpT6liKOAgW9ahJGETsN2F0s9G8zYezwmhjBJGp0rpjAwCF7G4stgOGHeP4W7wlxy+geckmtMarp5bny7TcIpurZJqWrpJQmpa6mJqWlpBBOSkTr8OiUJqWGlKmirY4i7CJTqaWlpaGkpxcSbAFcjby1L+YE5wFt62ggL11q8soIYTuKDwJYherpWPkFed3jyRvwQfa34S9tZXZqM4q6q32Glq2Qb/XxBWtz0pjQD1fzrsvf+ityIeY5G7eZBPJceXFbwZDsXxSxF4kBrOEcML0LWxthVoSQ068cHLfY+FwwKR1du+pvTkUMC6anpaSl");
        private static int[] order = new int[] { 7,12,4,8,4,11,12,7,13,13,12,12,13,13,14 };
        private static int key = 164;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
