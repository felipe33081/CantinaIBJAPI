using Amazon.CognitoIdentityProvider.Model;
using System;

namespace CantinaIBJ.Integration.Cognito
{
    public static class CognitoResponseExceptions
    {
        const string LABEL_ERROR_500 = "Erro de servidor";
        const string LABEL_ERROR_400 = "Erro de requisição";
        const string LABEL_ERROR_UNKNOWN = "Erro desconhecido";

        public static string GetFriendlyException(this Exception err)
        {
            var response = $"{LABEL_ERROR_UNKNOWN}. Entre em contato com a equipe de suporte.";

            if (err is InternalErrorException) response = $"{LABEL_ERROR_500}. Erro interno.";
            else if (err is InvalidParameterException) response = $"{LABEL_ERROR_400}. Parâmetro inválido.";
            else if (err is NotAuthorizedException) response = $"{LABEL_ERROR_400}. Não autorizado.";
            else if (err is ResourceNotFoundException) response = $"{LABEL_ERROR_400}. Recurso não encontrado.";
            else if (err is TooManyRequestsException) response = $"{LABEL_ERROR_400}. Limite excedido. Tente novamente mais tarde.";
            else if (err is GroupExistsException) response = $"{LABEL_ERROR_400}. O grupo já existe.";
            else if (err is LimitExceededException) response = $"{LABEL_ERROR_400}. Limite de usuários excedido.";
            else if (err is UserImportInProgressException) response = $"{LABEL_ERROR_400}. Importação de usuários em andamento. Tente novamente mais tarde.";
            else if (err is UserNotFoundException) response = $"{LABEL_ERROR_400}. Usuário não encontrado.";
            else if (err is CodeDeliveryFailureException) response = $"{LABEL_ERROR_400}. Falha na entrega do código de verificação.";
            else if (err is InvalidLambdaResponseException) response = $"{LABEL_ERROR_400}. Erro interno. Tente novamente mais tarde.";
            else if (err is InvalidPasswordException) response = $"{LABEL_ERROR_400}. Senha inválida.";
            else if (err is InvalidSmsRoleAccessPolicyException) response = $"{LABEL_ERROR_400}. Erro de SMS. Sem permissão de envio.";
            else if (err is InvalidSmsRoleTrustRelationshipException) response = $"{LABEL_ERROR_400}. Erro de SMS. Fonte não confiável.";
            else if (err is PreconditionNotMetException) response = $"{LABEL_ERROR_400}. Erro de validação.";
            else if (err is UnexpectedLambdaException) response = $"{LABEL_ERROR_400}. Erro durante validação da solicitação.";
            else if (err is UnsupportedUserStateException) response = $"{LABEL_ERROR_400}. Usuário em estado inválido. Entre em contato com a equipe de suporte";
            else if (err is UserLambdaValidationException) response = $"{LABEL_ERROR_400}. Erro durante validação do usuário. Entre em contato com a equipe de suporte.";
            else if (err is UsernameExistsException) response = $"{LABEL_ERROR_400}. Nome de usuário já existe.";
            else if (err is InvalidEmailRoleAccessPolicyException) response = $"{LABEL_ERROR_400}. E-mail informado não encontrado.";
            else if (err is PasswordResetRequiredException) response = $"{LABEL_ERROR_400}. O usuário precisa redefinir sua senha.";
            else if (err is UserNotConfirmedException) response = $"{LABEL_ERROR_400}. O usuário precisa confirmar sua conta.";

            return response;
        }
    }
}