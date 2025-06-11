pipeline {
   agent {
        kubernetes {
            defaultContainer 'shell'
            yaml """
                apiVersion: v1
                kind: Pod
                metadata:
                  labels:
                    some-label: some-value
                spec:
                  containers:
                  - name: shell
                    image: "public.ecr.aws/a2q7i5i2/sl-jenkins-base-ci:latest"
                    command:
                    - cat
                    tty: true

            """
        }
    }
    environment {
        IDENTIFIER = 'template.btq.sealights.co'
    }
     stages {
        stage("Uninstalling helm") {
            steps {
                script {
                    withCredentials([
                        sshUserPrivateKey(credentialsId: 'ssh-key', keyFileVariable: 'SSH_KEY_FILE', usernameVariable: 'SSH_USER')
                    ]) {
                        sh script: """
                            chmod 0400 ${SSH_KEY_FILE}
                            ssh -o StrictHostKeyChecking=no -i ${SSH_KEY_FILE} ${SSH_USER}@template.btq.sealights.co 'export KUBECONFIG=\$(k3d kubeconfig write btq) && helm uninstall btq'
                        """
                    }
                }
            }
        }
    }
}
