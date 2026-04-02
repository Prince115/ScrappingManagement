namespace ScrappingManagement.Web.Dto
{
    public class UserRoleEditDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> SelectedRoles { get; set; }
        public List<string> AllRoles { get; set; }
    }
}
