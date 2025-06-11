properties([
        parameters([
                string(name: 'tag', defaultValue: 'null', description: 'change btq version'),
                string(name: 'buildname', defaultValue: 'null', description: 'change sealights buildname for cd agents (if you change anything then this parameter is mandatory)'),
                string(name: 'labid', defaultValue: 'null', description: 'change sealights lab_id'),
                string(name: 'branch', defaultValue: 'null', description: 'change branch to pull (effects tests and helm)')
        ])
])

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
        IDENTIFIER = 'btq-template.btq.sealights.co'
        tag = "template_${params.tag}"
    }
     stages {
        stage("Preparing Spin up") {
            steps {
                script {
                    withCredentials([
                        string(credentialsId: 'sealights-token', variable: 'SL_TOKEN'),
                        sshUserPrivateKey(credentialsId: 'ssh-key', keyFileVariable: 'SSH_KEY_FILE', usernameVariable: 'SSH_USER')
                    ]) {
                        cleanWs()
                        ENV_NAME = "${IDENTIFIER}"
                        currentBuild.displayName = "${ENV_NAME} btq update"
                        LOWER_ENV_NAME = "${ENV_NAME}".toLowerCase()
                        IP = "${IDENTIFIER}"
                                stage("Updating Helm") {
                                    sh script: """
                                        chmod 0400 ${SSH_KEY_FILE}
                                        ssh -o StrictHostKeyChecking=no -i ${SSH_KEY_FILE} ${SSH_USER}@internal-template.btq.sealights.co 'bash /opt/sealights/install-btq.sh --tag=${env.tag} --buildname=${params.buildname} --labid=${params.labid} --branch=${params.branch} --token=${env.SL_TOKEN} --sl_branch=${params.branch}'
                                    """
                                }
                    }
                }
            }
        }
    }
}
